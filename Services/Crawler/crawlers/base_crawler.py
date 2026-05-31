from abc import ABC, abstractmethod

from redis import Redis
from requests import Session

from models.source_provider import CrawlTarget, SourceProvider


class BaseCrawler(ABC):
    def __init__(
        self,
        source_provider: SourceProvider,
        crawl_targets: tuple[CrawlTarget, ...],
        session: Session,
        redis_client: Redis,
    ) -> None:
        self.source_provider = source_provider
        self.crawl_targets = crawl_targets
        self.session = session
        self.redis_client = redis_client

    @abstractmethod
    def run(self) -> None:
        pass
