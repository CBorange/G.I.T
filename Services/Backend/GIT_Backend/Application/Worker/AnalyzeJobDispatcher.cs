using System.Globalization;
using GIT_Backend.Application.DTO;
using GIT_Backend.Application.Service;
using StackExchange.Redis;

namespace GIT_Backend.Application.Worker
{
    /// <summary>
    /// AnalyzerJob dispatch worker
    /// </summary>
    public sealed class AnalyzeJobDispatcher : BackgroundService
    {
        private const string StreamKey = "analyze-job-events";
        private const int BatchSize = 10;
        private const int StreamMaxLength = 1000;
        private static readonly TimeSpan PollingDelay = TimeSpan.FromSeconds(5);

        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AnalyzeJobDispatcher> _logger;

        public AnalyzeJobDispatcher(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory, ILogger<AnalyzeJobDispatcher> logger)
        {
            _redis = redis;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = _redis.GetDatabase();

            _logger.LogInformation(
                "Starting analyze job dispatcher. StreamKey={StreamKey}",
                StreamKey);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Wait Polling Delay");
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var analyzerService = scope.ServiceProvider.GetRequiredService<AnalyzerService>();
                    var messages = await analyzerService.GetDispatchableJobsAsync(BatchSize, stoppingToken);

                    if(messages.Count == 0)
                        _logger.LogInformation("No dispatchable analyze job found. skip dispatch at this time. StreamKey={StreamKey}", StreamKey);

                    foreach (var message in messages)
                    {
                        await DispatchAsync(db, analyzerService, message, stoppingToken);
                    }
                    await Task.Delay(PollingDelay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Analyze job dispatch loop failed.");
                    await Task.Delay(PollingDelay, stoppingToken);
                }
            }
        }

        private async Task DispatchAsync(IDatabase db, AnalyzerService analyzerService, 
            AnalyzeJobDispatchMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var dispatchedAt = DateTimeOffset.UtcNow;
                var streamId = await db.StreamAddAsync(StreamKey, ToStreamEntries(message, dispatchedAt), maxLength: StreamMaxLength, useApproximateMaxLength: true);
                var updated = await analyzerService.MarkJobDispatchedAsync(message.AnalyzeJobId, dispatchedAt, cancellationToken);

                if (!updated)
                {
                    _logger.LogWarning(
                        "Analyze job event was published but job status was not updated. StreamId={StreamId}, AnalyzeJobId={AnalyzeJobId}",
                        streamId,
                        message.AnalyzeJobId);
                    return;
                }

                _logger.LogInformation(
                    "Dispatched analyze job. StreamId={StreamId}, AnalyzeJobId={AnalyzeJobId}, RawContentId={RawContentId}, AnalyzerProviderId={AnalyzerProviderId}",
                    streamId,
                    message.AnalyzeJobId,
                    message.RawContentId,
                    message.AnalyzerProviderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Analyze job dispatch failed. AnalyzeJobId={AnalyzeJobId}, RawContentId={RawContentId}",
                    message.AnalyzeJobId,
                    message.RawContentId);
            }
        }

        private static NameValueEntry[] ToStreamEntries(AnalyzeJobDispatchMessage message, DateTimeOffset dispatchedAt)
        {
            var entries = new List<NameValueEntry>
            {
                new("analyze_job_id", message.AnalyzeJobId.ToString()),
                new("raw_content_id", message.RawContentId.ToString()),
                new("analyzer_provider_id", message.AnalyzerProviderId.ToString(CultureInfo.InvariantCulture)),
                new("analyzer_provider_code", message.AnalyzerProviderCode),
                new("analyzer_model_name", message.AnalyzerModelName),
                new("prompt_policy_code", message.PromptPolicyCode),
                new("attempt_count", message.AttemptCount.ToString(CultureInfo.InvariantCulture)),
                new("max_attempt_count", message.MaxAttemptCount.ToString(CultureInfo.InvariantCulture)),
                new("crawl_target_id", message.CrawlTargetId.ToString(CultureInfo.InvariantCulture)),
                new("source_url", message.SourceUrl),
                new("title", message.Title),
                new("crawled_at", FormatDateTimeOffset(message.CrawledAt)),
                new("dispatched_at", FormatDateTimeOffset(dispatchedAt)),
            };

            AddOptional(entries, "analyzer_endpoint_url", message.AnalyzerEndpointUrl);
            AddOptional(entries, "analyzer_config_json", message.AnalyzerConfigJson);
            AddOptional(entries, "content_id", message.ContentId);
            AddOptional(entries, "author", message.Author);
            AddOptional(entries, "published_at", message.PublishedAt is null ? null : FormatDateTimeOffset(message.PublishedAt.Value));
            AddOptional(entries, "body", message.Body);
            AddOptional(entries, "raw_payload_json", message.RawPayloadJson);

            return entries.ToArray();
        }

        private static void AddOptional(List<NameValueEntry> entries, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                entries.Add(new NameValueEntry(name, value));
            }
        }

        private static string FormatDateTimeOffset(DateTimeOffset value)
        {
            return value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }
    }
}
