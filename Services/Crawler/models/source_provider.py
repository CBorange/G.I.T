from dataclasses import dataclass


@dataclass(slots=True, frozen=True)
class SourceProvider:
    name: str
    code: str
    base_url: str
    interval_min: int
    request_delay_ms: int

    @staticmethod
    def map_source_provider(data: dict) -> "SourceProvider":
        return SourceProvider(
            name=data["name"],
            code=data["code"],
            base_url=data["baseUrl"],
            interval_min=data["intervalMin"],
            request_delay_ms=data["requestDelayMs"],
        )