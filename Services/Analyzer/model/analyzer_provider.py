from dataclasses import dataclass
from datetime import datetime
from typing import Any


@dataclass(slots=True, frozen=True)
class AnalyzerProvider:
    id: int
    name: str
    code: str
    model_name: str
    endpoint_url: str | None
    is_enabled: bool
    config_json: str | None
    created_at: datetime | None
    updated_at: datetime | None
    last_running_at: datetime | None

    @staticmethod
    def from_dict(data: dict[str, Any]) -> "AnalyzerProvider":
        return AnalyzerProvider(
            id=data["id"],
            name=data["name"],
            code=data["code"],
            model_name=data["modelName"],
            endpoint_url=data.get("endpointUrl"),
            is_enabled=data["isEnabled"],
            config_json=data.get("configJson"),
            created_at=parse_datetime(data.get("createdAt")),
            updated_at=parse_datetime(data.get("updatedAt")),
            last_running_at=parse_datetime(data.get("lastRunningAt")),
        )

    @staticmethod
    def from_stream_values(values: dict[str, str]) -> "AnalyzerProvider":
        code = values["analyzer_provider_code"]

        return AnalyzerProvider(
            id=int(values["analyzer_provider_id"]),
            name=code,
            code=code,
            model_name=values["analyzer_model_name"],
            endpoint_url=values.get("analyzer_endpoint_url"),
            is_enabled=True,
            config_json=values.get("analyzer_config_json"),
            created_at=None,
            updated_at=None,
            last_running_at=None,
        )


def parse_datetime(value: str | None) -> datetime | None:
    if value is None or value.strip() == "":
        return None

    return datetime.fromisoformat(value)
