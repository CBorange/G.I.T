from dataclasses import dataclass
from typing import Any


@dataclass(slots=True, frozen=True)
class SourceCategory:
    id: int
    code: str
    name: str
    description: str | None

    @staticmethod
    def from_dict(data: dict[str, Any]) -> "SourceCategory":
        return SourceCategory(
            id=int(data["id"]),
            code=data["code"],
            name=data["name"],
            description=data.get("description"),
        )
