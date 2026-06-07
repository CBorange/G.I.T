from dataclasses import dataclass
from datetime import datetime
from decimal import Decimal
from typing import Any
from uuid import UUID, uuid4


@dataclass(slots=True, frozen=True)
class AnalyzedContentResult:
    id: UUID
    raw_content_id: UUID
    analyzer_provider_id: int
    analyze_job_id: UUID
    actual_category_id: int
    title_summary: str
    body_summary: str
    keyword_json: str | None
    location_json: str | None
    model_name: str
    analysis_payload_json: str | None
    analyzed_at: datetime
    confidence: Decimal
    confidence_reason: str

    @staticmethod
    def create(
        raw_content_id: UUID,
        analyzer_provider_id: int,
        analyze_job_id: UUID,
        actual_category_id: int,
        title_summary: str,
        body_summary: str,
        keyword_json: str | None,
        location_json: str | None,
        model_name: str,
        analysis_payload_json: str | None,
        analyzed_at: datetime,
        confidence: Decimal,
        confidence_reason: str,
    ) -> "AnalyzedContentResult":
        return AnalyzedContentResult(
            id=uuid4(),
            raw_content_id=raw_content_id,
            analyzer_provider_id=analyzer_provider_id,
            analyze_job_id=analyze_job_id,
            actual_category_id=actual_category_id,
            title_summary=title_summary,
            body_summary=body_summary,
            keyword_json=keyword_json,
            location_json=location_json,
            model_name=model_name,
            analysis_payload_json=analysis_payload_json,
            analyzed_at=analyzed_at,
            confidence=confidence,
            confidence_reason=confidence_reason,
        )

    def to_redis_stream_values(self) -> dict[str, Any]:
        values: dict[str, Any] = {
            "id": str(self.id),
            "raw_content_id": str(self.raw_content_id),
            "analyzer_provider_id": self.analyzer_provider_id,
            "analyze_job_id": str(self.analyze_job_id),
            "actual_category_id": self.actual_category_id,
            "title_summary": self.title_summary,
            "body_summary": self.body_summary,
            "model_name": self.model_name,
            "analyzed_at": self.analyzed_at.isoformat(),
            "confidence": str(self.confidence),
            "confidence_reason": self.confidence_reason,
        }

        if self.keyword_json is not None:
            values["keyword_json"] = self.keyword_json
        if self.location_json is not None:
            values["location_json"] = self.location_json
        if self.analysis_payload_json is not None:
            values["analysis_payload_json"] = self.analysis_payload_json

        return values
