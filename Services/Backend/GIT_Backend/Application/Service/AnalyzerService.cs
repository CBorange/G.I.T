using GIT_Backend.Application.DTO;
using GIT_Backend.Domain.Constants;
using GIT_Backend.Domain.Entity;
using GIT_Backend.Infra.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GIT_Backend.Application.Service
{
    public class AnalyzerService
    {
        private const short DefaultMaxAttemptCount = 3;
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

        private Expression<Func<AnalyzeJob, bool>> IsDispatchableExpression()
        {
            var dispatchedThreshold = DateTimeOffset.UtcNow.AddSeconds(-5);
            var pending = AnalyzeJobStatus.Pending.ToString();

            var dispatched = AnalyzeJobStatus.Dispatched.ToString();
            var failed = AnalyzeJobStatus.Failed.ToString();
            var dispatchableStatuses = new[] { pending, dispatched, failed };

            return job =>
                job.Status == pending ||
                (job.Status == dispatched && job.LastRunningAt < dispatchedThreshold) ||
                (job.Status == failed && job.AttemptCount < (job.MaxAttemptCount ?? DefaultMaxAttemptCount));
        }

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
                    job.AnalyzerProvider.Code,
                    job.AnalyzerProvider.ModelName,
                    job.AnalyzerProvider.EndpointUrl,
                    job.AnalyzerProvider.ConfigJson,
                    job.PromptPolicyCode,
                    job.AttemptCount,
                    job.MaxAttemptCount ?? DefaultMaxAttemptCount,
                    job.RawContent.CrawlTargetId,
                    job.RawContent.SourceUrl,
                    job.RawContent.ContentId,
                    job.RawContent.Author,
                    job.RawContent.PublishedAt,
                    job.RawContent.Title,
                    job.RawContent.Body,
                    job.RawContent.RawPayloadJson,
                    job.RawContent.CrawledAt))
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
    }
}
