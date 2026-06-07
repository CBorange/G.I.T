from abc import ABC, abstractmethod

from redis import Redis
from requests import Session

from model.analyzer_provider import AnalyzerProvider


class BaseAnalyzer(ABC):
    def __init__(
        self,
        analyzer_provider: AnalyzerProvider,
        session: Session,
        redis_client: Redis,
    ) -> None:
        self.analyzer_provider = analyzer_provider
        self.session = session
        self.redis_client = redis_client

    @abstractmethod
    def run(self, message: dict[str, str]) -> None:
        pass
