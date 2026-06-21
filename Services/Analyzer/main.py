import logging
import socket
import time

from redis import Redis
from redis.exceptions import RedisError, ResponseError
from requests import RequestException, Session

from config.environment_loader import app_config
from config.log_config import configure_logging
from helper.http_client import create_session
from helper.redis_client import create_redis_client
from model.analyze_job_dispatch_message import AnalyzeJobDispatchMessage
from model.analyzer_provider import AnalyzerProvider
from model.source_category import SourceCategory
from service.analyzer_factory import create_analyzer

DEFAULT_BATCH_SIZE = 3
DEFAULT_BLOCK_MS = 5000
DEFAULT_LOOP_INTERVAL_SEC = 30
DEFAULT_PENDING_IDLE_MS = 1 * 60 * 1000


def ensure_consumer_group(redis_client: Redis) -> None:
    try:
        redis_client.xgroup_create(
            name=app_config.redis_disaptch_stream,
            groupname=app_config.redis_disaptch_consumer_group,
            id="0",
            mkstream=True,
        )
    except ResponseError as error:
        if "BUSYGROUP" not in str(error):
            raise


def read_analyze_job_entries(redis_client: Redis, consumer_name: str) -> list:
    entries = redis_client.xreadgroup(
        groupname=app_config.redis_disaptch_consumer_group,
        consumername=consumer_name,
        streams={app_config.redis_disaptch_stream: ">"},
        count=DEFAULT_BATCH_SIZE,
        block=DEFAULT_BLOCK_MS,
    )

    if not entries:
        return []

    return entries[0][1]


def claim_pending_analyze_job_entries(
    redis_client: Redis,
    consumer_name: str,
) -> list:
    claim_result = redis_client.xautoclaim(
        name=app_config.redis_disaptch_stream,
        groupname=app_config.redis_disaptch_consumer_group,
        consumername=consumer_name,
        min_idle_time=DEFAULT_PENDING_IDLE_MS,
        start_id="0-0",
        count=DEFAULT_BATCH_SIZE,
    )

    if not claim_result:
        return []

    return claim_result[1]


def load_analyzer_providers(
    session: Session,
    analyzer_provider_ids: list[int],
) -> dict[int, AnalyzerProvider]:
    if not analyzer_provider_ids:
        return {}

    logger = logging.getLogger(__name__)
    url = (
        f"{app_config.backend_api_base_url.rstrip('/')}"
        "/analyzer/analyzer-providers"
    )
    params = [
        ("ids", analyzer_provider_id)
        for analyzer_provider_id in analyzer_provider_ids
    ]

    try:
        response = session.get(
            url,
            params=params,
            timeout=app_config.backend_api_timeout_sec,
        )
        response.raise_for_status()
    except RequestException:
        logger.exception("Failed to request analyzer providers. url=%s", url)
        raise

    data = response.json()
    if not isinstance(data, list):
        raise RuntimeError("Backend analyzer-providers response must be a list.")

    analyzer_providers = [
        AnalyzerProvider.from_dict(item)
        for item in data
    ]

    return {
        analyzer_provider.id: analyzer_provider
        for analyzer_provider in analyzer_providers
    }


def load_source_categories(session: Session) -> list[SourceCategory]:
    logger = logging.getLogger(__name__)
    url = (
        f"{app_config.backend_api_base_url.rstrip('/')}"
        "/analyzer/source-categories"
    )

    try:
        response = session.get(
            url,
            timeout=app_config.backend_api_timeout_sec,
        )
        response.raise_for_status()
    except RequestException:
        logger.exception("Failed to request source categories. url=%s", url)
        raise

    data = response.json()
    if not isinstance(data, list):
        raise RuntimeError("Backend source-categories response must be a list.")

    return [
        SourceCategory.from_dict(item)
        for item in data
    ]


def parse_dispatch_messages(
    entries: list,
) -> list[tuple[str, AnalyzeJobDispatchMessage]]:
    return [
        (entry_id, AnalyzeJobDispatchMessage.from_stream_values(values))
        for entry_id, values in entries
    ]


def distinct_analyzer_provider_ids(
    messages: list[AnalyzeJobDispatchMessage],
) -> list[int]:
    analyzer_provider_ids = []
    seen_ids = set()

    for message in messages:
        if message.analyzer_provider_id in seen_ids:
            continue

        seen_ids.add(message.analyzer_provider_id)
        analyzer_provider_ids.append(message.analyzer_provider_id)

    return analyzer_provider_ids


def group_dispatch_messages_by_analyzer(
    dispatch_messages: list[tuple[str, AnalyzeJobDispatchMessage]],
) -> dict[tuple[int, str], list[tuple[str, AnalyzeJobDispatchMessage]]]:
    grouped_messages: dict[
        tuple[int, str],
        list[tuple[str, AnalyzeJobDispatchMessage]],
    ] = {}

    for entry_id, message in dispatch_messages:
        group_key = (
            message.analyzer_provider_id,
            message.prompt_policy_code,
        )
        if group_key not in grouped_messages:
            grouped_messages[group_key] = []

        grouped_messages[group_key].append((entry_id, message))

    return grouped_messages


def process_entries(
    redis_client: Redis,
    dispatch_messages: list[tuple[str, AnalyzeJobDispatchMessage]],
    analyzer_provider: AnalyzerProvider,
    source_categories: list[SourceCategory],
) -> None:
    entry_ids = [
        entry_id
        for entry_id, _ in dispatch_messages
    ]
    messages = [
        message
        for _, message in dispatch_messages
    ]

    analyzer = create_analyzer(analyzer_provider)
    analyzer.run(messages, source_categories)
    redis_client.xack(
        app_config.redis_disaptch_stream,
        app_config.redis_disaptch_consumer_group,
        *entry_ids,
    )


def main() -> None:
    configure_logging()
    logger = logging.getLogger(__name__)
    redis_client = create_redis_client()
    backend_session = create_session(
        headers={
            "X-Internal-Api-Key": app_config.internal_api_key,
        }
    )
    consumer_name = f"analyzer-{socket.gethostname()}"

    source_categories = load_source_categories(backend_session)
    ensure_consumer_group(redis_client)
    logger.info(
        (
            "Starting analyzer loop. stream_key=%s consumer_group=%s "
            "consumer=%s source_category_count=%s"
        ),
        app_config.redis_disaptch_stream,
        app_config.redis_disaptch_consumer_group,
        consumer_name,
        len(source_categories),
    )

    # Analyzer service supports loop mode only.
    while True:
        try:
            entries = claim_pending_analyze_job_entries(
                redis_client,
                consumer_name,
            )
            if entries:
                logger.info(
                    "Claimed pending analyze job events. count=%s",
                    len(entries),
                )
            else:
                entries = read_analyze_job_entries(redis_client, consumer_name)

            if not entries:
                logger.info("No analyze job event found.")
                continue

            dispatch_messages = parse_dispatch_messages(entries)
            analyzer_provider_ids = distinct_analyzer_provider_ids([
                message
                for _, message in dispatch_messages
            ])
            analyzer_providers = load_analyzer_providers(
                backend_session,
                analyzer_provider_ids,
            )

            grouped_dispatch_messages = group_dispatch_messages_by_analyzer(
                dispatch_messages
            )

            for group_key, grouped_messages in grouped_dispatch_messages.items():
                analyzer_provider_id, _ = group_key
                analyzer_provider = analyzer_providers.get(
                    analyzer_provider_id
                )
                if analyzer_provider is None:
                    raise RuntimeError(
                        "Analyzer provider not found. "
                        f"analyzer_provider_id={analyzer_provider_id}"
                    )

                process_entries(
                    redis_client,
                    grouped_messages,
                    analyzer_provider,
                    source_categories,
                )

        except RedisError:
            logger.exception("Redis error occurred during analyzer loop.")
            time.sleep(DEFAULT_LOOP_INTERVAL_SEC)
        except Exception:
            logger.exception("Error occurred during analyzer loop.")
            time.sleep(DEFAULT_LOOP_INTERVAL_SEC)


if __name__ == "__main__":
    main()
