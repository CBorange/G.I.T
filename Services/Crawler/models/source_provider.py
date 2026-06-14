from dataclasses import dataclass
from datetime import datetime
from typing import Any

@dataclass(slots=True, frozen=True)
class CrawlTarget:
    id: int
    code: str
    entry_url: str
    request_delay_ms: int | None

    @staticmethod
    def from_dict(data: dict[str, Any]) -> "CrawlTarget":
        return CrawlTarget(
            id=data["id"],
            code=data["code"],
            entry_url=data["entryUrl"],
            request_delay_ms=data.get("requestDelayMs"),
        )


@dataclass(slots=True, frozen=True)
class SourceProvider:
    id: int
    code: str
    base_url: str
    request_delay_ms: int
    crawl_targets: tuple[CrawlTarget, ...]

    @staticmethod
    def from_dict(data: dict[str, Any]) -> "SourceProvider":
        return SourceProvider(
            id=data["id"],
            code=data["code"],
            base_url=data["baseUrl"],
            request_delay_ms=data["requestDelayMs"],
            crawl_targets=tuple(
                CrawlTarget.from_dict(item)
                for item in data.get("crawlTargets", [])
            ),
        )