using GIT_Backend.Application.DTO;
using GIT_Backend.Application.Service;
using GIT_Backend.Domain.Entity;
using StackExchange.Redis;
using System.Globalization;

namespace GIT_Backend.Application.Worker
{
    /// <summary>
    /// Consumes analyzed content results from the Analyzer service and persists final content.
    /// </summary>
    public sealed class AnalyzedContentsConsumer : BackgroundService
    {
        private const string StreamKey = "analyzed_result-events";
        private const string GroupName = "analyzed_result-group";

        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AnalyzedContentsConsumer> _logger;

        private const int BatchSize = 10;
        private const long ClaimMinIdleTimeMs = 1 * 60 * 1000;
        private const string ClaimStartId = "0-0";

        public AnalyzedContentsConsumer(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory, ILogger<AnalyzedContentsConsumer> logger)
        {
            _redis = redis;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var db = _redis.GetDatabase();

                _logger.LogInformation(
                    "Starting analyzed result Redis stream consumer. StreamKey={StreamKey}, GroupName={GroupName}",
                    StreamKey,
                    GroupName);

                await EnsureConsumerGroupAsync(db);

                var consumerName = $"{Environment.MachineName}-{Guid.NewGuid():N}";
                _logger.LogInformation("Analyzed result Redis stream consumer is polling. ConsumerName={ConsumerName}", consumerName);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var entries = await ClaimPendingEntriesAsync(db, consumerName);
                        if (entries.Length > 0)
                        {
                            _logger.LogInformation(
                                "Claimed pending analyzed result events. Count={Count}",
                                entries.Length);
                        }
                        else
                        {
                            entries = await db.StreamReadGroupAsync(
                                StreamKey,
                                GroupName,
                                consumerName,
                                ">",
                                count: BatchSize);
                        }

                        if (entries.Length == 0)
                        {
                            await Task.Delay(1000, stoppingToken);
                            continue;
                        }

                        await using var scope = _scopeFactory.CreateAsyncScope();
                        var analyzerService = scope.ServiceProvider.GetRequiredService<AnalyzerService>();

                        foreach (var entry in entries)
                        {
                            try
                            {
                                await ProcessAsync(analyzerService, entry, stoppingToken);
                                await db.StreamAcknowledgeAsync(StreamKey, GroupName, entry.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex,
                                    "Analyzed result message processing failed. EntryId={EntryId}",
                                    entry.Id);
                            }
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Start analyzed result Redis polling failed.");

                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task EnsureConsumerGroupAsync(IDatabase db)
        {
            try
            {
                await db.StreamCreateConsumerGroupAsync(
                    StreamKey,
                    GroupName,
                    position: "0-0",
                    createStream: true);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
            {
                _logger.LogInformation("Analyzed result Redis stream consumer group already exists. GroupName={GroupName}", GroupName);
            }
        }

        private async Task<StreamEntry[]> ClaimPendingEntriesAsync(IDatabase db, string consumerName)
        {
            var claimResult = await db.StreamAutoClaimAsync(
                StreamKey,
                GroupName,
                consumerName,
                ClaimMinIdleTimeMs,
                ClaimStartId,
                BatchSize);

            if (claimResult.IsNull)
            {
                return [];
            }

            return claimResult.ClaimedEntries;
        }

        private async Task ProcessAsync(AnalyzerService analyzerService, StreamEntry entry, CancellationToken cancellationToken)
        {
            var values = entry.Values.ToDictionary(
                x => x.Name.ToString(),
                x => x.Value.ToString());

            AnalyzedContentResultMessage message;
            try
            {
                message = ParseMessage(values);
            }
            catch (Exception ex) when (ex is InvalidOperationException or FormatException or OverflowException)
            {
                await HandleInvalidMessageAsync(analyzerService, entry, values, ex, cancellationToken);
                return;
            }

            var result = await analyzerService.SaveAnalyzedContentAsync(message, cancellationToken);
            if (result.Succeeded && result.JobUpdated)
            {
                _logger.LogInformation(
                    "Saved analyzed content result. EntryId={EntryId}, AnalyzeJobId={AnalyzeJobId}, RawContentId={RawContentId}, AnalyzedContentId={AnalyzedContentId}",
                    entry.Id,
                    message.AnalyzeJobId,
                    message.RawContentId,
                    result.AnalyzedContentId);
                return;
            }

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "Analyzed content result was already completed. EntryId={EntryId}, AnalyzeJobId={AnalyzeJobId}, Reason={Reason}",
                    entry.Id,
                    message.AnalyzeJobId,
                    result.FailureReason);
                return;
            }

            _logger.LogWarning(
                "Analyzed content result was rejected. EntryId={EntryId}, AnalyzeJobId={AnalyzeJobId}, JobUpdated={JobUpdated}, Reason={Reason}",
                entry.Id,
                message.AnalyzeJobId,
                result.JobUpdated,
                result.FailureReason);
        }

        private async Task HandleInvalidMessageAsync(
            AnalyzerService analyzerService,
            StreamEntry entry,
            Dictionary<string, string> values,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (!(values.TryGetValue("analyze_job_id", out var value) && Guid.TryParse(value, out Guid analyzeJobId)))
            {
                _logger.LogWarning(exception,
                    "Invalid analyzed result message skipped. EntryId={EntryId}, Reason=AnalyzeJobIdMissingOrInvalid",
                    entry.Id);
                return;
            }

            var reason = $"Invalid analyzed result message. entry_id={entry.Id}, error={exception.Message}";
            var result = await analyzerService.MarkAnalyzeJobDeadAsync(analyzeJobId, reason, cancellationToken);

            _logger.LogWarning(exception,
                "Invalid analyzed result message skipped. EntryId={EntryId}, AnalyzeJobId={AnalyzeJobId}, JobUpdated={JobUpdated}, Reason={Reason}",
                entry.Id,
                analyzeJobId,
                result.JobUpdated,
                result.FailureReason);
        }

        private AnalyzedContentResultMessage ParseMessage(Dictionary<string, string> values)
        {
            return new AnalyzedContentResultMessage(
                Id: Guid.Parse(values.GetRequiredField("id")),
                RawContentId: Guid.Parse(values.GetRequiredField("raw_content_id")),
                AnalyzerProviderId: short.Parse(values.GetRequiredField("analyzer_provider_id"), CultureInfo.InvariantCulture),
                AnalyzeJobId: Guid.Parse(values.GetRequiredField("analyze_job_id")),
                ActualCategoryId: short.Parse(values.GetRequiredField("actual_category_id"), CultureInfo.InvariantCulture),
                TitleSummary: values.GetRequiredField("title_summary"),
                BodySummary: values.GetRequiredField("body_summary"),
                KeywordJson: values.GetOptionalField("keyword_json"),
                LocationJson: values.GetOptionalField("location_json"),
                ModelName: values.GetRequiredField("model_name"),
                AnalysisPayloadJson: values.GetOptionalField("analysis_payload_json"),
                AnalyzedAt: DateTimeOffset.Parse(
                    values.GetRequiredField("analyzed_at"),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                Confidence: decimal.Parse(values.GetRequiredField("confidence"), CultureInfo.InvariantCulture),
                ConfidenceReason: values.GetRequiredField("confidence_reason"));
        }
    }
}
