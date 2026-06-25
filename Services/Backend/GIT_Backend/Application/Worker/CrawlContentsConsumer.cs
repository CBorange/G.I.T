using System.Globalization;
using GIT_Backend.Application.DTO;
using GIT_Backend.Application.Service;
using StackExchange.Redis;

namespace GIT_Backend.Application.Worker
{
    public sealed class CrawlContentsConsumer : BackgroundService
    {
        private const string StreamKey = "raw-content-events";
        private const string GroupName = "backend-raw-content-group";
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<CrawlContentsConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private const int BatchSize = 10;
        private const long ClaimMinIdleTimeMs = 1 * 60 * 1000;
        private const string ClaimStartId = "0-0";

        public CrawlContentsConsumer(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory, ILogger<CrawlContentsConsumer> logger)
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
                        var entries = await ClaimPendingEntriesAsync(db, consumerName);
                        if (entries.Length > 0)
                        {
                            _logger.LogInformation(
                                "Claimed pending raw content events. Count={Count}",
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
                        var crawlerService = scope.ServiceProvider.GetRequiredService<CrawlerService>();
                        foreach (var entry in entries)
                        {
                            try
                            {
                                await ProcessAsync(crawlerService, entry, stoppingToken);
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

                // prevent reconnect storm
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

        private async Task ProcessAsync(CrawlerService crawlerService, StreamEntry entry, CancellationToken cancellationToken)
        {
            var values = entry.Values.ToDictionary(
                x => x.Name.ToString(),
                x => x.Value.ToString());

            var message = ParseMessage(values);
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

        private CrawlerRawContentMessage ParseMessage(Dictionary<string, string> values)
        {
            return new CrawlerRawContentMessage(
                Id: Guid.Parse(values.GetRequiredField("id")),
                CrawlTargetId: int.Parse(values.GetRequiredField("crawl_target_id"), CultureInfo.InvariantCulture),
                SourceUrl: values.GetRequiredField("source_url"),
                ContentId: values.GetRequiredField("content_id"),
                Author: values.GetRequiredField("author"),
                PublishedAt: values.GetOptionalDateTimeOffsetField("published_at"),
                Title: values.GetRequiredField("title"),
                Body: values.GetRequiredField("body"),
                RawPayloadJson: values.GetOptionalField("raw_payload_json"),
                CrawledAt: DateTimeOffset.Parse(
                    values.GetRequiredField("crawled_at"),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind));
        }

    }
}
