from crawlers.base_crawler import BaseCrawler
from crawlers.seoul_mediahub_cralwer import SeoulMediahubCrawler
from helper.http_client import create_session
from models.source_provider import SourceProvider

CRAWLER_BY_PROVIDER_CODE: dict[str, type[BaseCrawler]] = {
    "seoul_mediahub": SeoulMediahubCrawler,
}


def create_crawler(source_provider: SourceProvider) -> BaseCrawler:
    crawler_type = CRAWLER_BY_PROVIDER_CODE.get(source_provider.code.lower())
    if crawler_type is None:
        raise ValueError(
            f"Unsupported crawler provider code: {source_provider.code}"
        )

    return crawler_type(
        source_provider=source_provider,
        crawl_targets=source_provider.crawl_targets,
        session=create_session(),
    )
