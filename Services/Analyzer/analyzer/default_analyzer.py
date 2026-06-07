import logging

from analyzer.base_analyzer import BaseAnalyzer
from model.analyzed_content_result import AnalyzedContentResult

logger = logging.getLogger(__name__)


class DefaultAnalyzer(BaseAnalyzer):
    def run(self, message: dict[str, str]) -> None:
        logger.info(
            "Analyzer requested. provider=%s analyze_job_id=%s raw_content_id=%s",
            self.analyzer_provider.code,
            message.get("analyze_job_id"),
            message.get("raw_content_id"),
        )

        result = self._analyze(message)
        self._publish_result(result)

    def _analyze(self, message: dict[str, str]) -> AnalyzedContentResult:
        raise NotImplementedError("Default analyzer logic is not implemented.")

    def _publish_result(self, result: AnalyzedContentResult) -> None:
        raise NotImplementedError("Analyzer result publishing is not implemented.")
