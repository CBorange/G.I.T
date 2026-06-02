from dataclasses import dataclass
from datetime import date


@dataclass(slots=True, frozen=True)
class CrawlDateRange:
    start_date: date
    end_date: date

    def start_date_text(self) -> str:
        return self.start_date.isoformat()

    def end_date_text(self) -> str:
        return self.end_date.isoformat()
