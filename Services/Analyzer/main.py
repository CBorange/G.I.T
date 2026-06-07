import logging
import socket
import time

from redis import Redis
from redis.exceptions import RedisError, ResponseError

from config.environment_loader import app_config
from config.log_config import configure_logging
from helper.redis_client import create_redis_client
from model.analyzer_provider import AnalyzerProvider
from service.analyzer_factory import create_analyzer

DEFAULT_BATCH_SIZE = 10
DEFAULT_BLOCK_MS = 5000
DEFAULT_LOOP_INTERVAL_SEC = 5


def ensure_consumer_group(redis_client: Redis) -> None:
    try:
        redis_client.xgroup_create(
            name=app_config.redis_stream_key,
            groupname=app_config.redis_consumer_group,
            id="0",
            mkstream=True,
        )
    except ResponseError as error:
        if "BUSYGROUP" not in str(error):
            raise


def read_analyze_job_entries(redis_client: Redis, consumer_name: str) -> list:
    entries = redis_client.xreadgroup(
        groupname=app_config.redis_consumer_group,
        consumername=consumer_name,
        streams={app_config.redis_stream_key: ">"},
        count=DEFAULT_BATCH_SIZE,
        block=DEFAULT_BLOCK_MS,
    )

    if not entries:
        return []

    return entries[0][1]


def process_entry(redis_client: Redis, entry_id: str, values: dict[str, str]) -> None:
    analyzer_provider = AnalyzerProvider.from_stream_values(values)
    analyzer = create_analyzer(analyzer_provider)
    analyzer.run(values)
    redis_client.xack(
        app_config.redis_stream_key,
        app_config.redis_consumer_group,
        entry_id,
    )


def main() -> None:
    configure_logging()
    logger = logging.getLogger(__name__)
    redis_client = create_redis_client()
    consumer_name = f"analyzer-{socket.gethostname()}"

    ensure_consumer_group(redis_client)
    logger.info(
        "Starting analyzer loop. stream_key=%s consumer_group=%s consumer=%s",
        app_config.redis_stream_key,
        app_config.redis_consumer_group,
        consumer_name,
    )

    # Analyzer service supports loop mode only.
    while True:
        try:
            entries = read_analyze_job_entries(redis_client, consumer_name)
            if not entries:
                logger.info("No analyze job event found.")
                continue

            for entry_id, values in entries:
                process_entry(redis_client, entry_id, values)

        except RedisError:
            logger.exception("Redis error occurred during analyzer loop.")
            time.sleep(DEFAULT_LOOP_INTERVAL_SEC)
        except Exception:
            logger.exception("Error occurred during analyzer loop.")
            time.sleep(DEFAULT_LOOP_INTERVAL_SEC)


if __name__ == "__main__":
    main()
