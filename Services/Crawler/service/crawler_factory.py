from crawlers.base_crawler import BaseCrawler
from crawlers.seoul_mediahub_crawler import SeoulMediahubCrawler
from helper.http_client import create_crawler_session
from helper.redis_client import create_redis_client
from models.crawl_date_range import CrawlDateRange
from models.source_provider import SourceProvider

CRAWLER_BY_PROVIDER_CODE: dict[str, type[BaseCrawler]] = {
    "seoul_mediahub": SeoulMediahubCrawler,
}


def create_crawler(
    source_provider: SourceProvider,
    crawl_date_range: CrawlDateRange,
) -> BaseCrawler:
    crawler_type = CRAWLER_BY_PROVIDER_CODE.get(source_provider.code.lower())
    if crawler_type is None:
        raise ValueError(
            f"Unsupported crawler provider code: {source_provider.code}"
        )

    return crawler_type(
        source_provider=source_provider,
        crawl_targets=source_provider.crawl_targets,
        crawl_date_range=crawl_date_range,
        session=create_crawler_session(),
        redis_client=create_redis_client(),
    )
