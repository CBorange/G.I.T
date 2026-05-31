using System.Globalization;
using GIT_Backend.Application.DTO;
using GIT_Backend.Application.Service;
using StackExchange.Redis;

namespace GIT_Backend.Application.Worker
{
    public sealed class RawContentConsumerWorker : BackgroundService
    {
        private const string StreamKey = "raw-content-events";
        private const string GroupName = "backend-raw-content-group";
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RawContentConsumerWorker> _logger;

        public RawContentConsumerWorker(
            IConnectionMultiplexer redis,
            IServiceScopeFactory scopeFactory,
            ILogger<RawContentConsumerWorker> logger)
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
                    "Starting Redis stream consumer. StreamKey={StreamKey}, GroupName={GroupName}",
                    StreamKey,
                    GroupName);

                await EnsureConsumerGroupAsync(db);

                var consumerName = $"{Environment.MachineName}-{Guid.NewGuid():N}";
                _logger.LogInformation("Redis stream consumer is polling. ConsumerName={ConsumerName}", consumerName);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var entries = await db.StreamReadGroupAsync(
                            StreamKey,
                            GroupName,
                            consumerName,
                            ">",
                            count: 10);

                        if (entries.Length == 0)
                        {
                            await Task.Delay(1000, stoppingToken);
                            continue;
                        }

                        foreach (var entry in entries)
                        {
                            try
                            {
                                await ProcessAsync(entry, stoppingToken);

                                await db.StreamAcknowledgeAsync(StreamKey, GroupName, entry.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex,
                                    "Message processing failed. EntryId={EntryId}",
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
                _logger.LogError(ex, "Start Redis polling failed.");

                // reconnect storm ¹æÁö
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
                _logger.LogInformation("Redis stream consumer group already exists. GroupName={GroupName}", GroupName);
            }
        }

        private async Task ProcessAsync(StreamEntry entry, CancellationToken cancellationToken)
        {
            var values = entry.Values.ToDictionary(
                x => x.Name.ToString(),
                x => x.Value.ToString());

            var message = ParseMessage(values);

            await using var scope = _scopeFactory.CreateAsyncScope();
            var crawlerService = scope.ServiceProvider.GetRequiredService<CrawlerService>();
            var result = await crawlerService.SaveRawContentAsync(message, cancellationToken);

            if (result.Created)
            {
                _logger.LogInformation(
                    "Saved raw content and analyze job. EntryId={EntryId}, RawContentId={RawContentId}, AnalyzeJobId={AnalyzeJobId}, Url={SourceUrl}, Title={Title}",
                    entry.Id,
                    result.RawContentId,
                    result.AnalyzeJobId,
                    message.SourceUrl,
                    message.Title);
                return;
            }

            _logger.LogInformation(
                "Raw content already exists. EntryId={EntryId}, RawContentId={RawContentId}, Url={SourceUrl}, Title={Title}",
                entry.Id,
                result.RawContentId,
                message.SourceUrl,
                message.Title);
        }

        private static CrawlerRawContentMessage ParseMessage(Dictionary<string, string> values)
        {
            return new CrawlerRawContentMessage(
                Id: Guid.Parse(GetRequiredValue(values, "id")),
                CrawlTargetId: int.Parse(GetRequiredValue(values, "crawl_target_id"), CultureInfo.InvariantCulture),
                SourceUrl: GetRequiredValue(values, "source_url"),
                ContentId: GetOptionalValue(values, "content_id"),
                Author: GetOptionalValue(values, "author"),
                PublishedAt: ParseOptionalDateTimeOffset(values, "published_at"),
                Title: GetRequiredValue(values, "title"),
                Body: GetOptionalValue(values, "body"),
                RawPayloadJson: GetOptionalValue(values, "raw_payload_json"),
                CrawledAt: DateTimeOffset.Parse(
                    GetRequiredValue(values, "crawled_at"),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind));
        }

        private static string GetRequiredValue(Dictionary<string, string> values, string key)
        {
            if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Required Redis stream field is missing. field={key}");
            }

            return value;
        }

        private static string? GetOptionalValue(Dictionary<string, string> values, string key)
        {
            return values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : null;
        }

        private static DateTimeOffset? ParseOptionalDateTimeOffset(Dictionary<string, string> values, string key)
        {
            var value = GetOptionalValue(values, key);
            if (value is null)
            {
                return null;
            }

            return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }
    }
}
