import logging

from crawlers.base_crawler import BaseCrawler

logger = logging.getLogger(__name__)


class SeoulMediahubCrawler(BaseCrawler):
    def run(self) -> None:
        logger.info(
            "Crawler requested. provider=%s target_count=%s",
            self.source_provider.code,
            len(self.crawl_targets),
        )
