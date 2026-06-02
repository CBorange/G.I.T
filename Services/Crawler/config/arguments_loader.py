import argparse
import logging
import re
from datetime import date, datetime, timedelta, timezone

from models.crawl_date_range import CrawlDateRange

LOOKBACK_PATTERN = re.compile(r"^(?P<amount>[1-9]\d*)(?P<unit>y|mo|w|d|h)$")
DEFAULT_LOOKBACK = "7d"
SEOUL_TIMEZONE = timezone(timedelta(hours=9))

mode = "loop"
provider: str | None = None


def resolve_crawl_date_range(
    lookback: str | None,
    from_date: str | None,
    now: datetime | None = None,
) -> CrawlDateRange:
    base_datetime = now or datetime.now(SEOUL_TIMEZONE)
    end_date = base_datetime.date()
    if lookback is not None and from_date is not None:
        raise ValueError("--lookback and --from cannot be used together")

    if from_date is not None:
        start_date = _parse_from_date(from_date)
    else:
        start_date = _resolve_lookback_start_date(
            lookback or DEFAULT_LOOKBACK,
            base_datetime,
        )

    if start_date > end_date:
        raise ValueError("crawl start date cannot be later than today")

    return CrawlDateRange(start_date=start_date, end_date=end_date)


def _parse_from_date(value: str) -> date:
    try:
        return datetime.strptime(value, "%Y-%m-%d").date()
    except ValueError as exc:
        raise ValueError("--from must be yyyy-MM-dd format") from exc


def _resolve_lookback_start_date(value: str, base_datetime: datetime) -> date:
    match = LOOKBACK_PATTERN.match(value)
    if match is None:
        raise ValueError(
            "--lookback must be a duration like 1y, 6mo, 3w, 30d, 12h"
        )

    amount = int(match.group("amount"))
    unit = match.group("unit")
    end_date = base_datetime.date()
    if unit == "y":
        return _add_months(end_date, -(amount * 12))
    if unit == "mo":
        return _add_months(end_date, -amount)
    if unit == "w":
        return end_date - timedelta(weeks=amount)
    if unit == "d":
        return end_date - timedelta(days=amount)

    return (base_datetime - timedelta(hours=amount)).date()


def _add_months(base_date: date, months: int) -> date:
    month_index = base_date.month - 1 + months
    year = base_date.year + month_index // 12
    month = month_index % 12 + 1
    day = min(base_date.day, _days_in_month(year, month))
    return date(year, month, day)


def _days_in_month(year: int, month: int) -> int:
    next_month = date(year + month // 12, month % 12 + 1, 1)
    return (next_month - timedelta(days=1)).day


def load_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Get Environment Arguments")
    parser.add_argument(
        "--mode",
        type=str,
        help="crawler execution mode(once, loop)",
        default="loop",
        choices=("once", "loop"),
    )
    parser.add_argument(
        "--provider",
        type=str,
        help="provider code for once mode",
        required=False,
    )
    parser.add_argument(
        "--lookback",
        type=str,
        help="crawl date range duration from today. e.g. 1y, 6mo, 3w, 30d, 12h",
        required=False,
    )
    parser.add_argument(
        "--from",
        dest="from_date",
        type=str,
        help="crawl date range start date from today. format: yyyy-MM-dd",
        required=False,
    )
    args = parser.parse_args()

    global mode, provider
    mode = args.mode
    provider = args.provider
    if mode == "once" and provider is None:
        parser.error("--provider is required when --mode once")
    try:
        resolve_crawl_date_range(
            lookback=args.lookback,
            from_date=args.from_date,
        )
    except ValueError as exc:
        parser.error(str(exc))

    logging.info(f"====== Crawler Service Mode: {mode} =======")
    return args
