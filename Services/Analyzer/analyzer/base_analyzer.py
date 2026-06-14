from abc import ABC, abstractmethod

from redis import Redis
from requests import Session

from model.analyze_job_dispatch_message import AnalyzeJobDispatchMessage
from model.analyzed_content_result import AnalyzedContentResult
from model.analyzer_provider import AnalyzerProvider
from model.source_category import SourceCategory


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
    def run(
        self,
        messages: list[AnalyzeJobDispatchMessage],
        source_categories: list[SourceCategory],
    ) -> list[AnalyzedContentResult]:
        pass
