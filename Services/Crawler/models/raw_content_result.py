from dataclasses import dataclass
from datetime import datetime
from typing import Any
from uuid import UUID, uuid4


@dataclass(slots=True, frozen=True)
class ArticleLink:
    source_url: str
    content_id: str
    list_item_html: str


@dataclass(slots=True, frozen=True)
class RawContentResult:
    id: UUID
    crawl_target_id: int
    source_url: str
    content_id: str | None
    author: str | None
    published_at: datetime | None
    title: str
    body: str | None
    raw_payload_json: str
    crawled_at: datetime

    @staticmethod
    def create(
        crawl_target_id: int,
        source_url: str,
        content_id: str | None,
        author: str | None,
        published_at: datetime | None,
        title: str,
        body: str | None,
        raw_payload_json: str,
        crawled_at: datetime,
    ) -> "RawContentResult":
        return RawContentResult(
            id=uuid4(),
            crawl_target_id=crawl_target_id,
            source_url=source_url,
            content_id=content_id,
            author=author,
            published_at=published_at,
            title=title,
            body=body,
            raw_payload_json=raw_payload_json,
            crawled_at=crawled_at,
        )

    def to_redis_stream_values(self) -> dict[str, Any]:
        values: dict[str, Any] = {
            "id": str(self.id),
            "crawl_target_id": self.crawl_target_id,
            "source_url": self.source_url,
            "title": self.title,
            "raw_payload_json": self.raw_payload_json,
            "crawled_at": self.crawled_at.isoformat(),
        }

        if self.content_id is not None:
            values["content_id"] = self.content_id
        if self.author is not None:
            values["author"] = self.author
        if self.published_at is not None:
            values["published_at"] = self.published_at.isoformat()
        if self.body is not None:
            values["body"] = self.body

        return values
