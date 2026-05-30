using StackExchange.Redis;

namespace GIT_Backend.Application.Worker
{
    public sealed class RawContentConsumerWorker : BackgroundService
    {
        private const string StreamKey = "raw-content-events";
        private const string GroupName = "backend-raw-content-group";
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RawContentConsumerWorker> _logger;

        public RawContentConsumerWorker(IConnectionMultiplexer redis, ILogger<RawContentConsumerWorker> logger)
        {
            _redis = redis;
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

                // reconnect storm 방지
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

        private Task ProcessAsync(StreamEntry entry, CancellationToken cancellationToken)
        {
            var values = entry.Values.ToDictionary(
                x => x.Name.ToString(),
                x => x.Value.ToString());

            var source_url = values.GetValueOrDefault("source_url");
            var title = values.GetValueOrDefault("title");

            _logger.LogInformation("Consumed article. Url={source_url}, Title={title}", source_url, title);

            // TODO: 데이터 검증

            return Task.CompletedTask;
        }
    }
}
