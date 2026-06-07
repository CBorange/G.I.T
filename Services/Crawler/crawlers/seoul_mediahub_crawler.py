import logging
import time
from datetime import datetime, timedelta, timezone

from redis.exceptions import RedisError
from requests import RequestException

from config.environment_loader import app_config
from crawlers.base_crawler import BaseCrawler
from service.seoul_mediahub_parser import (
    build_article_list_payload,
    parse_article,
    parse_article_links,
    parse_last_page,
)
from models.raw_content_result import ArticleLink, RawContentResult
from models.source_provider import CrawlTarget

logger = logging.getLogger(__name__)

SEOUL_TIMEZONE = timezone(timedelta(hours=9))
DEFAULT_STREAM_MAXLEN = 10000

class SeoulMediahubCrawler(BaseCrawler):
    def run(self) -> None:
        logger.info(
            "Crawler requested. provider=%s target_count=%s",
            self.source_provider.code,
            len(self.crawl_targets),
        )

        for crawl_target in self.crawl_targets:
            self._crawl_target(crawl_target)

    def _crawl_target(self, crawl_target: CrawlTarget) -> None:
        logger.info(
            "Start crawl target. target_id=%s target_code=%s entry_url=%s",
            crawl_target.id,
            crawl_target.code,
            crawl_target.entry_url,
        )

        try:
            first_page_html = self._request_article_list_page(crawl_target, 1)
            last_page = parse_last_page(first_page_html)
        except RequestException:
            logger.exception(
                "Failed to resolve crawl page range. target_id=%s",
                crawl_target.id,
            )
            return

        logger.info(
            "Resolved crawl page range. target_id=%s last_page=%s",
            crawl_target.id,
            last_page,
        )

        for page_no in range(1, last_page + 1):
            try:
                html = (
                    first_page_html
                    if page_no == 1
                    else self._request_article_list_page(crawl_target, page_no)
                )
                article_links = parse_article_links(
                    html=html,
                    base_url=self.source_provider.base_url,
                )
            except RequestException:
                logger.exception(
                    "Failed to request article list. target_id=%s page_no=%s",
                    crawl_target.id,
                    page_no,
                )
                continue

            logger.info(
                "Parsed article list. target_id=%s page_no=%s article_count=%s",
                crawl_target.id,
                page_no,
                len(article_links),
            )

            for article_link in article_links:
                self._wait_before_article_request(crawl_target)
                self._crawl_article(crawl_target, article_link, page_no)

    def _request_article_list_page(
        self,
        crawl_target: CrawlTarget,
        page_no: int,
    ) -> str:
        payload = build_article_list_payload(
            crawl_target,
            page_no,
            self.crawl_date_range,
        )
        response = self.session.post(
            crawl_target.entry_url,
            data=payload,
            timeout=app_config.backend_api_timeout_sec,
        )
        response.raise_for_status()
        return response.text

    def _crawl_article(
        self,
        crawl_target: CrawlTarget,
        article_link: ArticleLink,
        page_no: int,
    ) -> None:
        try:
            response = self.session.get(
                article_link.source_url,
                timeout=app_config.backend_api_timeout_sec,
            )
            response.raise_for_status()

            result = parse_article(
                html=response.text,
                crawl_target_id=crawl_target.id,
                source_url=article_link.source_url,
                content_id=article_link.content_id,
                list_item_html=article_link.list_item_html,
                entry_url=crawl_target.entry_url,
                page_no=page_no,
                crawled_at=datetime.now(SEOUL_TIMEZONE),
            )
            self._publish_result(result)
        except (RequestException, RedisError, ValueError):
            logger.exception(
                "Failed to crawl article. target_id=%s source_url=%s",
                crawl_target.id,
                article_link.source_url,
            )

    def _publish_result(self, result: RawContentResult) -> None:
        self.redis_client.xadd(
            app_config.redis_stream_key,
            result.to_redis_stream_values(),
            maxlen=DEFAULT_STREAM_MAXLEN,
            approximate=True,
        )
        logger.info(
            "Published raw content. stream_key=%s source_url=%s title=%s",
            app_config.redis_stream_key,
            result.source_url,
            result.title,
        )

    def _wait_before_article_request(self, crawl_target: CrawlTarget) -> None:
        delay_ms = (
            crawl_target.request_delay_ms
            if crawl_target.request_delay_ms is not None
            else self.source_provider.request_delay_ms
        )
        if delay_ms <= 0:
            return

        time.sleep(delay_ms / 1000)

