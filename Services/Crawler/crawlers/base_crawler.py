from abc import ABC, abstractmethod

from requests import Session

from models.source_provider import CrawlTarget, SourceProvider


class BaseCrawler(ABC):
    def __init__(
        self,
        source_provider: SourceProvider,
        crawl_targets: tuple[CrawlTarget, ...],
        session: Session,
    ) -> None:
        self.source_provider = source_provider
        self.crawl_targets = crawl_targets
        self.session = session

    @abstractmethod
    def run(self) -> None:
        pass
