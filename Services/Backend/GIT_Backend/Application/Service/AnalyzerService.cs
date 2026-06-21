using GIT_Backend.Application.DTO;
using GIT_Backend.Domain.Constants;
using GIT_Backend.Domain.Entity;
using GIT_Backend.Infra.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;

namespace GIT_Backend.Application.Service
{
    public class AnalyzerService
    {
        private const short DefaultMaxAttemptCount = 3;
        private const decimal MinimumAcceptedConfidence = 0.65m;
        private readonly GITDBContext _dbContext;
        private readonly ILogger<AnalyzerService> _logger;

        public AnalyzerService(ILogger<AnalyzerService> logger, GITDBContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<AnalyzerProviderResponse>> GetAnalyzerProvidersAsync(
            IReadOnlyCollection<short> analyzerProviderIds,
            CancellationToken cancellationToken)
        {
            if (analyzerProviderIds.Count == 0)
            {
                return [];
            }

            var distinctAnalyzerProviderIds = analyzerProviderIds.Distinct().ToArray();

            return await _dbContext.AnalyzerProviders
                .AsNoTracking()
                .Where(analyzerProvider => distinctAnalyzerProviderIds.Contains(analyzerProvider.Id) && analyzerProvider.IsEnabled)
                .OrderBy(analyzerProvider => analyzerProvider.Id)
                .Select(analyzerProvider => new AnalyzerProviderResponse(
                    analyzerProvider.Id,
                    analyzerProvider.Name,
                    analyzerProvider.Code,
                    analyzerProvider.ModelName,
                    analyzerProvider.EndpointUrl,
                    analyzerProvider.IsEnabled,
                    analyzerProvider.ConfigJson,
                    analyzerProvider.CreatedAt,
                    analyzerProvider.UpdatedAt,
                    analyzerProvider.LastRunningAt))
                .ToListAsync(cancellationToken);
        }

        #region Analyzer Job Dispatch
        public async Task<IReadOnlyList<AnalyzeJobDispatchMessage>> GetDispatchableJobsAsync(int batchSize, CancellationToken cancellationToken)
        {
            var isDispatchable = IsDispatchableExpression();

            return await _dbContext.AnalyzeJobs
                .AsNoTracking()
                .Where(isDispatchable)
                .OrderBy(job => job.LastRunningAt ?? DateTimeOffset.MinValue)
                .ThenBy(job => job.Id)
                .Take(batchSize)
                .Select(job => new AnalyzeJobDispatchMessage(
                    job.Id,
                    job.RawContentId,
                    job.AnalyzerProviderId,
                    job.PromptPolicyCode,
                    job.RawContent.Title,
                    job.RawContent.Body))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> MarkJobDispatchedAsync(Guid analyzeJobId, DateTimeOffset dispatchedAt, CancellationToken cancellationToken)
        {
            var isDispatchable = IsDispatchableExpression();
            string dispatched = AnalyzeJobStatus.Dispatched.ToString();
            try
            {
                var updatedCount = await _dbContext.AnalyzeJobs
                    .Where(job => job.Id == analyzeJobId)
                    .Where(isDispatchable)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(job => job.Status, dispatched)
                        .SetProperty(job => job.LastRunningAt, dispatchedAt.ToUniversalTime()),
                        cancellationToken);

                return updatedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update dispatched analyze job. AnalyzeJobId={AnalyzeJobId}", analyzeJobId);
                throw;
            }
        }
        private Expression<Func<AnalyzeJob, bool>> IsDispatchableExpression()
        {
            var dispatchedThreshold = DateTimeOffset.UtcNow.AddHours(-1);
            var pending = AnalyzeJobStatus.Pending.ToString();

            var dispatched = AnalyzeJobStatus.Dispatched.ToString();
            var failed = AnalyzeJobStatus.Failed.ToString();
            var dispatchableStatuses = new[] { pending, dispatched, failed };

            return job =>
                job.Status == pending ||
                (job.Status == dispatched && job.LastRunningAt < dispatchedThreshold) ||
                (job.Status == failed && job.AttemptCount < (job.MaxAttemptCount ?? DefaultMaxAttemptCount));
        }
        #endregion

        #region Analyzed Contents Consume
        public async Task<AnalyzedContentProcessResult> SaveAnalyzedContentAsync(
            AnalyzedContentResultMessage message,
            CancellationToken cancellationToken)
        {
            var completedAt = DateTimeOffset.UtcNow;

            try
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                var analyzeJob = await _dbContext.AnalyzeJobs
                    .SingleOrDefaultAsync(job => job.Id == message.AnalyzeJobId, cancellationToken);

                if (analyzeJob is null)
                {
                    return new AnalyzedContentProcessResult(
                        message.AnalyzeJobId,
                        AnalyzedContentId: null,
                        Succeeded: false,
                        JobUpdated: false,
                        FailureReason: "AnalyzeJob not found.");
                }

                if (IsTerminalStatus(analyzeJob.Status))
                {
                    return new AnalyzedContentProcessResult(
                        message.AnalyzeJobId,
                        AnalyzedContentId: null,
                        Succeeded: analyzeJob.Status == AnalyzeJobStatus.Succeeded.ToString(),
                        JobUpdated: false,
                        FailureReason: $"AnalyzeJob is already terminal. status={analyzeJob.Status}");
                }

                var validationError = await ValidateAnalyzedContentAsync(analyzeJob, message, cancellationToken);
                if (validationError is not null)
                {
                    MarkAnalyzeJobDead(analyzeJob, validationError, completedAt);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return new AnalyzedContentProcessResult(
                        analyzeJob.Id,
                        AnalyzedContentId: null,
                        Succeeded: false,
                        JobUpdated: true,
                        FailureReason: validationError);
                }

                var analyzedContent = new AnalyzedContent
                {
                    Id = message.Id,
                    RawContentId = message.RawContentId,
                    AnalyzerProviderId = message.AnalyzerProviderId,
                    AnalyzeJobId = message.AnalyzeJobId,
                    ActualCategoryId = message.ActualCategoryId,
                    TitleSummary = message.TitleSummary,
                    BodySummary = message.BodySummary,
                    KeywordJson = message.KeywordJson,
                    LocationJson = message.LocationJson,
                    ModelName = message.ModelName,
                    AnalysisPayloadJson = message.AnalysisPayloadJson,
                    AnalyzedAt = message.AnalyzedAt.ToUniversalTime(),
                    CreatedAt = completedAt,
                    Confidence = message.Confidence,
                    ConfidenceReason = message.ConfidenceReason,
                };

                _dbContext.AnalyzedContents.Add(analyzedContent);
                MarkAnalyzeJobSucceeded(analyzeJob, completedAt);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new AnalyzedContentProcessResult(
                    analyzeJob.Id,
                    analyzedContent.Id,
                    Succeeded: true,
                    JobUpdated: true,
                    FailureReason: null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to save analyzed content result. AnalyzeJobId={AnalyzeJobId}, RawContentId={RawContentId}",
                    message.AnalyzeJobId,
                    message.RawContentId);
                throw;
            }
        }

        public async Task<AnalyzedContentProcessResult> MarkAnalyzeJobDeadAsync(
            Guid analyzeJobId,
            string lastError,
            CancellationToken cancellationToken)
        {
            var completedAt = DateTimeOffset.UtcNow;

            try
            {
                var analyzeJob = await _dbContext.AnalyzeJobs
                    .SingleOrDefaultAsync(job => job.Id == analyzeJobId, cancellationToken);

                if (analyzeJob is null)
                {
                    return new AnalyzedContentProcessResult(
                        analyzeJobId,
                        AnalyzedContentId: null,
                        Succeeded: false,
                        JobUpdated: false,
                        FailureReason: "AnalyzeJob not found.");
                }

                if (IsTerminalStatus(analyzeJob.Status))
                {
                    return new AnalyzedContentProcessResult(
                        analyzeJobId,
                        AnalyzedContentId: null,
                        Succeeded: analyzeJob.Status == AnalyzeJobStatus.Succeeded.ToString(),
                        JobUpdated: false,
                        FailureReason: $"AnalyzeJob is already terminal. status={analyzeJob.Status}");
                }

                MarkAnalyzeJobDead(analyzeJob, lastError, completedAt);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new AnalyzedContentProcessResult(
                    analyzeJob.Id,
                    AnalyzedContentId: null,
                    Succeeded: false,
                    JobUpdated: true,
                    FailureReason: lastError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark analyze job dead. AnalyzeJobId={AnalyzeJobId}", analyzeJobId);
                throw;
            }
        }

        private async Task<string?> ValidateAnalyzedContentAsync(
            AnalyzeJob analyzeJob,
            AnalyzedContentResultMessage message,
            CancellationToken cancellationToken)
        {
            if (message.Confidence is < 0m or > 1m)
            {
                return $"Confidence is out of range. confidence={message.Confidence}";
            }

            if (message.Confidence < MinimumAcceptedConfidence)
            {
                return $"Confidence is below minimum accepted threshold. confidence={message.Confidence}";
            }

            if (analyzeJob.RawContentId != message.RawContentId)
            {
                return $"RawContentId mismatch. expected={analyzeJob.RawContentId}, actual={message.RawContentId}";
            }

            if (analyzeJob.AnalyzerProviderId != message.AnalyzerProviderId)
            {
                return $"AnalyzerProviderId mismatch. expected={analyzeJob.AnalyzerProviderId}, actual={message.AnalyzerProviderId}";
            }

            var actualCategoryExists = await _dbContext.SourceCategories
                .AsNoTracking()
                .AnyAsync(category => category.Id == message.ActualCategoryId, cancellationToken);

            if (!actualCategoryExists)
            {
                return $"Actual category not found. actual_category_id={message.ActualCategoryId}";
            }

            var alreadySaved = await _dbContext.AnalyzedContents
                .AsNoTracking()
                .AnyAsync(content =>
                    content.Id == message.Id ||
                    content.RawContentId == message.RawContentId ||
                    content.AnalyzeJobId == message.AnalyzeJobId,
                    cancellationToken);

            if (alreadySaved)
            {
                return $"Analyzed content already exists. analyze_job_id={message.AnalyzeJobId}";
            }

            return ValidateJsonPayload(message.KeywordJson, "keyword_json")
                ?? ValidateJsonPayload(message.LocationJson, "location_json")
                ?? ValidateJsonPayload(message.AnalysisPayloadJson, "analysis_payload_json");
        }

        private string? ValidateJsonPayload(string? payload, string fieldName)
        {
            if (payload is null)
            {
                return null;
            }

            try
            {
                using (JsonDocument.Parse(payload))
                {
                    return null;
                }
            }
            catch (JsonException)
            {
                return $"Invalid JSON payload. field={fieldName}";
            }
        }

        private bool IsTerminalStatus(string status)
        {
            return status == AnalyzeJobStatus.Succeeded.ToString()
                || status == AnalyzeJobStatus.Dead.ToString();
        }

        private void MarkAnalyzeJobSucceeded(AnalyzeJob analyzeJob, DateTimeOffset completedAt)
        {
            analyzeJob.Status = AnalyzeJobStatus.Succeeded.ToString();
            analyzeJob.AttemptCount = IncrementAttemptCount(analyzeJob.AttemptCount);
            analyzeJob.LastError = null;
            analyzeJob.EndedAt = completedAt;
        }

        private void MarkAnalyzeJobDead(AnalyzeJob analyzeJob, string lastError, DateTimeOffset completedAt)
        {
            analyzeJob.Status = AnalyzeJobStatus.Dead.ToString();
            analyzeJob.AttemptCount = IncrementAttemptCount(analyzeJob.AttemptCount);
            analyzeJob.LastError = lastError;
            analyzeJob.EndedAt = completedAt;
        }

        private short IncrementAttemptCount(short attemptCount)
        {
            return attemptCount == short.MaxValue ? short.MaxValue : (short)(attemptCount + 1);
        }
        #endregion

        public async Task<IReadOnlyList<SourceCategoryResponse>> GetActiveCategoriesAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.SourceCategories
                .AsNoTracking()
                .Where(sourceCategory => sourceCategory.IsActive)
                .OrderBy(sourceCategory => sourceCategory.Id)
                .Select(sourceCategory => new SourceCategoryResponse(
                    sourceCategory.Id,
                    sourceCategory.Code,
                    sourceCategory.Name,
                    sourceCategory.Description))
                .ToListAsync(cancellationToken);
        }
    }
}
