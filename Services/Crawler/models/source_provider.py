from dataclasses import dataclass
from datetime import datetime
from typing import Any

@dataclass(slots=True, frozen=True)
class CrawlTarget:
    id: int
    source_category_id: int
    name: str
    code: str
    entry_url: str
    request_delay_ms: int | None
    is_active: bool
    last_running_at: datetime | None

    @staticmethod
    def from_dict(data: dict[str, Any]) -> "CrawlTarget":
        return CrawlTarget(
            id=data["id"],
            source_category_id=data["sourceCategoryId"],
            name=data["name"],
            code=data["code"],
            entry_url=data["entryUrl"],
            request_delay_ms=data.get("requestDelayMs"),
            is_active=data["isActive"],
            last_running_at=datetime.fromisoformat(data["lastRunningAt"])
            if data.get("lastRunningAt")
            else None,
        )


@dataclass(slots=True, frozen=True)
class SourceProvider:
    id: int
    name: str
    code: str
    base_url: str
    is_active: bool
    request_delay_ms: int
    description: str | None
    last_running_at: datetime | None
    crawl_targets: tuple[CrawlTarget, ...]

    @staticmethod
    def from_dict(data: dict[str, Any]) -> "SourceProvider":
        return SourceProvider(
            id=data["id"],
            name=data["name"],
            code=data["code"],
            base_url=data["baseUrl"],
            is_active=data["isActive"],
            request_delay_ms=data["requestDelayMs"],
            description=data.get("description"),
            last_running_at=datetime.fromisoformat(data["lastRunningAt"])
            if data.get("lastRunningAt")
            else None,
            crawl_targets=tuple(
                CrawlTarget.from_dict(item)
                for item in data.get("crawlTargets", [])
            ),
        )