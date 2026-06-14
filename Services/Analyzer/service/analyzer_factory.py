from analyzer.base_analyzer import BaseAnalyzer
from analyzer.default_analyzer import DefaultAnalyzer
from helper.http_client import create_session
from helper.redis_client import create_redis_client
from model.analyzer_provider import AnalyzerProvider

ANALYZER_BY_PROVIDER_CODE: dict[str, type[BaseAnalyzer]] = {
    "default_analyzer": DefaultAnalyzer,
}

def create_analyzer(analyzer_provider: AnalyzerProvider) -> BaseAnalyzer:
    analyzer_type = ANALYZER_BY_PROVIDER_CODE.get(
        analyzer_provider.code.lower()
    )
    if analyzer_type is None:
        raise ValueError(
            f"Unsupported analyzer provider code: {analyzer_provider.code}"
        )

    return analyzer_type(
        analyzer_provider=analyzer_provider,
        session=create_session(),
        redis_client=create_redis_client(),
    )
