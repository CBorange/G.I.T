import logging
import time

from requests import RequestException

from config.arguments_loader import load_arguments
from config.environment_loader import app_config
from config.log_config import configure_logging
from crawlers.base_crawler import BaseCrawler
from helper.http_client import create_session
from models.source_provider import SourceProvider
from service.crawler_factory import create_crawler


def load_source_providers() -> list[SourceProvider]:
    logger = logging.getLogger(__name__)
    session = create_session()
    url = (
        f"{app_config.backend_api_base_url.rstrip('/')}"
        "/crawler/source-providers"
    )

    try:
        response = session.get(url, timeout=app_config.backend_api_timeout_sec)
        response.raise_for_status()
    except RequestException:
        logger.exception("Failed to request source providers. url=%s", url)
        raise

    data = response.json()
    if not isinstance(data, list):
        raise RuntimeError("Backend source-providers response must be a list.")

    return [
        SourceProvider.from_dict(item)
        for item in data
    ]


def get_source_provider(
    source_providers: list[SourceProvider],
    provider_code: str,
) -> SourceProvider:
    for source_provider in source_providers:
        if source_provider.code.lower() == provider_code.lower():
            return source_provider

    raise ValueError(
        f"SourceProvider not found. provider_code={provider_code}"
    )

def main() -> None:
    configure_logging()
    args = load_arguments()

    logger = logging.getLogger(__name__)
    interval_sec: int = 3600

    # once 모드는 crawler 지정해서 실행
    if args.mode == "once":
        try:
            logger.info("Running crawler in 'once' mode.")
            source_providers = load_source_providers()
            source_provider = get_source_provider(
                source_providers=source_providers,
                provider_code=args.provider,
            )
            crawler = create_crawler(source_provider)
            crawler.run()
        except Exception as e:
            logger.error(f"Error occurred during crawler running once: {e}")
        return

    # loop 모드는 interval_sec마다 전체 crawler 실행, API 호출해서 크롤러 구현체 DB 정보로부터 생성
    while True:
        try:
            logger.info("Starting crawler iteration.")
            source_providers = load_source_providers()

            for source_provider in source_providers:
                crawler = create_crawler(source_provider)
                crawler.run()

            logger.info("Crawler iteration completed.")

        except Exception as e:
            logger.error(f"Error occurred during crawler iteration: {e}")

        time.sleep(interval_sec)


if __name__ == "__main__":
    main()
