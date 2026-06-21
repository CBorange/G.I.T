import json
import logging
from collections import Counter
from datetime import datetime, timezone
from decimal import Decimal, InvalidOperation
from pathlib import Path
from typing import Any
from uuid import UUID

from openai import OpenAI, OpenAIError
from redis.exceptions import RedisError

from analyzer.base_analyzer import BaseAnalyzer
from config.environment_loader import app_config
from model.analyze_job_dispatch_message import AnalyzeJobDispatchMessage
from model.analyzed_content_result import AnalyzedContentResult
from model.source_category import SourceCategory

logger = logging.getLogger(__name__)

DEFAULT_PROMPT_POLICY_CODE = "common_analyze_policy"
MAX_ANALYSIS_BATCH_SIZE = 3
PROMPT_DIR = Path(__file__).resolve().parent / "prompts"
SYSTEM_PROMPT_PATH = PROMPT_DIR / "system.md"
PROMPT_POLICY_PATHS = {
    DEFAULT_PROMPT_POLICY_CODE: PROMPT_DIR / "policies" / "common_v1.md",
}


class DefaultAnalyzer(BaseAnalyzer):
    def run(
        self,
        messages: list[AnalyzeJobDispatchMessage],
        source_categories: list[SourceCategory],
    ) -> list[AnalyzedContentResult]:
        if not messages:
            return []
        if len(messages) > MAX_ANALYSIS_BATCH_SIZE:
            raise ValueError(
                "DefaultAnalyzer supports up to "
                f"{MAX_ANALYSIS_BATCH_SIZE} messages per LLM request."
            )

        logger.info(
            (
                "Analyzer batch requested. provider=%s batch_size=%s "
                "source_category_count=%s"
            ),
            self.analyzer_provider.code,
            len(messages),
            len(source_categories),
        )

        try:
            results = self._analyze(messages, source_categories)
        except Exception:
            logger.exception(
                "Failed to analyze message batch. provider=%s batch_size=%s",
                self.analyzer_provider.code,
                len(messages),
            )
            raise

        self._publish_results(results)
        return results

    def _analyze(
        self,
        messages: list[AnalyzeJobDispatchMessage],
        source_categories: list[SourceCategory],
    ) -> list[AnalyzedContentResult]:
        prompt_policy_code = resolve_prompt_policy_code(messages)
        system_prompt = load_prompt(SYSTEM_PROMPT_PATH)
        prompt_policy = load_prompt_policy(prompt_policy_code)
        user_prompt = build_user_prompt(
            prompt_policy=prompt_policy,
            source_categories=source_categories,
            messages=messages,
        )
        response_text = self._request_llm(system_prompt, user_prompt)
        analysis_payload = parse_llm_json(response_text)

        return build_analyzed_results(
            analyzer_provider_id=self.analyzer_provider.id,
            model_name=self.analyzer_provider.model_name,
            messages=messages,
            source_categories=source_categories,
            analysis_payload=analysis_payload,
        )

    def _request_llm(self, system_prompt: str, user_prompt: str) -> str:
        client = OpenAI(api_key=app_config.openai_api_key)

        try:
            response = client.responses.create(
                model=self.analyzer_provider.model_name,
                instructions=system_prompt,
                input=user_prompt,
                temperature=0.2,
                text={"format": {"type": "json_object"}},
            )
        except OpenAIError:
            logger.exception(
                "Failed to request LLM analysis. provider=%s model=%s",
                self.analyzer_provider.code,
                self.analyzer_provider.model_name,
            )
            raise
        except Exception:
            logger.exception(
                "Unexpected error during LLM analysis request. "
                "provider=%s model=%s",
                self.analyzer_provider.code,
                self.analyzer_provider.model_name,
            )
            raise

        response_text = response.output_text
        if response_text is None or response_text.strip() == "":
            raise RuntimeError("LLM analysis response is empty.")

        return response_text

    def _publish_results(self, results: list[AnalyzedContentResult]) -> None:
        for result in results:
            self._publish_result(result)

    def _publish_result(self, result: AnalyzedContentResult) -> None:
        try:
            self.redis_client.xadd(
                app_config.redis_analyzed_stream,
                result.to_redis_stream_values(),
            )
        except RedisError:
            logger.exception(
                "Failed to publish analyzer result. stream_key=%s "
                "analyze_job_id=%s raw_content_id=%s",
                app_config.redis_analyzed_stream,
                result.analyze_job_id,
                result.raw_content_id,
            )
            raise

        logger.info(
            "Published analyzer result. stream_key=%s analyze_job_id=%s "
            "raw_content_id=%s",
            app_config.redis_analyzed_stream,
            result.analyze_job_id,
            result.raw_content_id,
        )


def resolve_prompt_policy_code(
    messages: list[AnalyzeJobDispatchMessage],
) -> str:
    prompt_policy_codes = {
        (
            (message.prompt_policy_code or DEFAULT_PROMPT_POLICY_CODE)
            .strip()
            .lower()
            or DEFAULT_PROMPT_POLICY_CODE
        )
        for message in messages
    }
    if len(prompt_policy_codes) != 1:
        raise ValueError(
            "DefaultAnalyzer batch must use a single prompt policy code."
        )

    return prompt_policy_codes.pop()


def load_prompt(path: Path) -> str:
    return path.read_text(encoding="utf-8").strip()


def load_prompt_policy(prompt_policy_code: str) -> str:
    prompt_path = PROMPT_POLICY_PATHS.get(prompt_policy_code)
    if prompt_path is None:
        raise ValueError(
            f"Unsupported prompt policy code: {prompt_policy_code}"
        )

    return load_prompt(prompt_path)


def build_user_prompt(
    prompt_policy: str,
    source_categories: list[SourceCategory],
    messages: list[AnalyzeJobDispatchMessage],
) -> str:
    prompt_input = {
        "prompt_policy": prompt_policy,
        "source_categories": [
            {
                "id": source_category.id,
                "code": source_category.code,
                "name": source_category.name,
                "description": source_category.description,
            }
            for source_category in source_categories
        ],
        "messages": [
            {
                "analyze_job_id": message.analyze_job_id,
                "raw_content_id": message.raw_content_id,
                "title": message.title,
                "body": message.body,
            }
            for message in messages
        ],
    }

    return (
        "아래 JSON 입력을 system prompt와 prompt_policy에 따라 분석하고, "
        "지정된 최종 JSON 형식만 반환하라.\n"
        f"{json.dumps(prompt_input, ensure_ascii=False)}"
    )


def parse_llm_json(response_text: str) -> dict[str, Any]:
    try:
        payload = json.loads(strip_json_fence(response_text))
    except json.JSONDecodeError as error:
        raise RuntimeError("LLM analysis response is not valid JSON.") from error

    if not isinstance(payload, dict):
        raise RuntimeError("LLM analysis response must be a JSON object.")
    if not isinstance(payload.get("results"), list):
        raise RuntimeError("LLM analysis response must contain results array.")

    return payload


def strip_json_fence(response_text: str) -> str:
    stripped_text = response_text.strip()
    if not stripped_text.startswith("```"):
        return stripped_text

    lines = stripped_text.splitlines()
    if len(lines) >= 2 and lines[0].startswith("```") and lines[-1] == "```":
        return "\n".join(lines[1:-1]).strip()

    return stripped_text


def build_analyzed_results(
    analyzer_provider_id: int,
    model_name: str,
    messages: list[AnalyzeJobDispatchMessage],
    source_categories: list[SourceCategory],
    analysis_payload: dict[str, Any],
) -> list[AnalyzedContentResult]:
    analysis_results = analysis_payload["results"]
    validate_analysis_result_identity(messages, analysis_results)

    result_by_job_id = {
        str(item.get("analyze_job_id")): item
        for item in analysis_results
    }
    source_category_ids = {
        source_category.id
        for source_category in source_categories
    }
    analyzed_at = datetime.now(timezone.utc)

    return [
        build_analyzed_result(
            analyzer_provider_id=analyzer_provider_id,
            model_name=model_name,
            message=message,
            source_category_ids=source_category_ids,
            analysis_item=result_by_job_id.get(message.analyze_job_id),
            analyzed_at=analyzed_at,
        )
        for message in messages
    ]


def validate_analysis_result_identity(
    messages: list[AnalyzeJobDispatchMessage],
    analysis_results: list[Any],
) -> None:
    if len(analysis_results) != len(messages):
        raise RuntimeError(
            "LLM analysis result count does not match message count. "
            f"expected_count={len(messages)} actual_count={len(analysis_results)}"
        )

    invalid_indexes = [
        index
        for index, item in enumerate(analysis_results)
        if not isinstance(item, dict)
    ]
    if invalid_indexes:
        raise RuntimeError(
            "LLM analysis results must contain only JSON objects. "
            f"invalid_indexes={invalid_indexes}"
        )

    expected_analyze_job_ids = [
        message.analyze_job_id
        for message in messages
    ]
    actual_analyze_job_ids = get_analysis_result_identity_values(
        analysis_results,
        "analyze_job_id",
    )
    validate_exact_identity_set(
        "analyze_job_id",
        expected_analyze_job_ids,
        actual_analyze_job_ids,
    )

    expected_raw_content_ids = [
        message.raw_content_id
        for message in messages
    ]
    actual_raw_content_ids = get_analysis_result_identity_values(
        analysis_results,
        "raw_content_id",
    )
    validate_exact_identity_set(
        "raw_content_id",
        expected_raw_content_ids,
        actual_raw_content_ids,
    )

    if actual_analyze_job_ids != expected_analyze_job_ids:
        raise RuntimeError(
            "LLM analysis result order does not match input message order. "
            "field=analyze_job_id "
            f"expected_order={expected_analyze_job_ids} "
            f"actual_order={actual_analyze_job_ids}"
        )


def get_analysis_result_identity_values(
    analysis_results: list[Any],
    field_name: str,
) -> list[str]:
    return [
        str(item.get(field_name))
        for item in analysis_results
    ]


def validate_exact_identity_set(
    field_name: str,
    expected_values: list[str],
    actual_values: list[str],
) -> None:
    expected_value_set = set(expected_values)
    actual_value_set = set(actual_values)
    actual_value_counts = Counter(actual_values)

    missing = [
        value
        for value in expected_values
        if value not in actual_value_set
    ]
    unknown = unique_preserving_order(
        [
            value
            for value in actual_values
            if value not in expected_value_set
        ]
    )
    duplicated = [
        value
        for value, count in actual_value_counts.items()
        if count > 1
    ]

    if missing or unknown or duplicated:
        raise RuntimeError(
            "LLM analysis result identity mismatch. "
            f"field={field_name} missing={missing} unknown={unknown} "
            f"duplicated={duplicated}"
        )


def unique_preserving_order(values: list[str]) -> list[str]:
    seen = set()
    result = []
    for value in values:
        if value in seen:
            continue
        seen.add(value)
        result.append(value)

    return result


def build_analyzed_result(
    analyzer_provider_id: int,
    model_name: str,
    message: AnalyzeJobDispatchMessage,
    source_category_ids: set[int],
    analysis_item: dict[str, Any] | None,
    analyzed_at: datetime,
) -> AnalyzedContentResult:
    if analysis_item is None:
        raise RuntimeError(
            "LLM analysis result is missing for analyze_job_id="
            f"{message.analyze_job_id}"
        )
    if str(analysis_item.get("raw_content_id")) != message.raw_content_id:
        raise RuntimeError(
            "LLM analysis result raw_content_id does not match input. "
            f"analyze_job_id={message.analyze_job_id}"
        )

    actual_category_id = coerce_int(
        analysis_item.get("actual_category_id"),
        "actual_category_id",
    )
    if actual_category_id not in source_category_ids:
        raise RuntimeError(
            "LLM analysis selected unknown source category. "
            f"actual_category_id={actual_category_id}"
        )

    return AnalyzedContentResult.create(
        raw_content_id=UUID(message.raw_content_id),
        analyzer_provider_id=analyzer_provider_id,
        analyze_job_id=UUID(message.analyze_job_id),
        actual_category_id=actual_category_id,
        title_summary=coerce_string(
            analysis_item.get("title_summary"),
            "title_summary",
        ),
        body_summary=coerce_string(
            analysis_item.get("body_summary"),
            "body_summary",
        ),
        keyword_json=json.dumps(
            coerce_string_list(analysis_item.get("keyword_json"), "keyword_json"),
            ensure_ascii=False,
        ),
        location_json=json.dumps(
            coerce_string_list(analysis_item.get("location_json"), "location_json"),
            ensure_ascii=False,
        ),
        model_name=model_name,
        analysis_payload_json=json.dumps(analysis_item, ensure_ascii=False),
        analyzed_at=analyzed_at,
        confidence=coerce_confidence(analysis_item.get("confidence")),
        confidence_reason=coerce_string(
            analysis_item.get("confidence_reason"),
            "confidence_reason",
        ),
    )


def coerce_int(value: Any, field_name: str) -> int:
    if isinstance(value, bool):
        raise RuntimeError(f"{field_name} must be an integer.")

    try:
        return int(value)
    except (TypeError, ValueError) as error:
        raise RuntimeError(f"{field_name} must be an integer.") from error


def coerce_string(value: Any, field_name: str) -> str:
    if not isinstance(value, str) or value.strip() == "":
        raise RuntimeError(f"{field_name} must be a non-empty string.")

    return value.strip()


def coerce_string_list(value: Any, field_name: str) -> list[str]:
    if isinstance(value, str):
        try:
            value = json.loads(value)
        except json.JSONDecodeError as error:
            raise RuntimeError(f"{field_name} must be a JSON array.") from error

    if not isinstance(value, list):
        raise RuntimeError(f"{field_name} must be a JSON array.")

    result = [
        item.strip()
        for item in value
        if isinstance(item, str) and item.strip() != ""
    ]
    if len(result) != len(value):
        raise RuntimeError(f"{field_name} must contain only strings.")

    return result


def coerce_confidence(value: Any) -> Decimal:
    try:
        confidence = Decimal(str(value))
    except (InvalidOperation, ValueError) as error:
        raise RuntimeError("confidence must be a number.") from error

    if confidence < Decimal("0.0"):
        return Decimal("0.0")
    if confidence > Decimal("1.0"):
        return Decimal("1.0")

    return confidence
