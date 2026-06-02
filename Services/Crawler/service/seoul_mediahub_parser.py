import json
import re
from datetime import datetime, timedelta, timezone
from urllib.parse import urljoin

from bs4 import BeautifulSoup, Tag

from models.raw_content_result import ArticleLink, RawContentResult

SEOUL_TIMEZONE = timezone(timedelta(hours=9))


def parse_last_page(html: str) -> int:
    soup = BeautifulSoup(html, "html.parser")
    last_link = soup.select_one("li.arrow.last a[onclick]")
    if last_link is None:
        return 1

    onclick = last_link.get("onclick", "")
    match = re.search(r"goList\((\d+)\)", onclick)
    if match is None:
        return 1

    return int(match.group(1))


def parse_article_links(html: str, base_url: str) -> list[ArticleLink]:
    soup = BeautifulSoup(html, "html.parser")
    article_list = soup.select_one("ul.news_summary_list")
    if article_list is None:
        return []

    links: list[ArticleLink] = []
    for item in article_list.find_all("li", recursive=False):
        if not isinstance(item, Tag):
            continue

        anchor = item.select_one("div.info a[href]")
        if anchor is None:
            continue

        href = anchor.get("href")
        content_id = _extract_content_id(href)
        if href is None or content_id is None:
            continue

        links.append(
            ArticleLink(
                source_url=urljoin(base_url, href),
                content_id=content_id,
                list_item_html=str(item),
            )
        )

    return links


def parse_article(
    html: str,
    crawl_target_id: int,
    source_url: str,
    content_id: str,
    list_item_html: str,
    entry_url: str,
    page_no: int,
    crawled_at: datetime,
) -> RawContentResult:
    soup = BeautifulSoup(html, "html.parser")

    header = soup.select_one("div.news_detail_view div.news_detail_top")
    if header is None:
        raise ValueError(f"Article header not found. source_url={source_url}")

    title_tag = header.select_one("h1.tit")
    if title_tag is None:
        raise ValueError(f"Article title not found. source_url={source_url}")

    writer_tag = header.select_one("p.writer a") or header.select_one("p.writer")
    published_at_tag = header.select_one("p.date span.num")
    body_root = _find_body_root(soup)
    story_tags = body_root.select("div.article div.story_area") if body_root else []

    title = _clean_text(title_tag.get_text(" ", strip=True))
    author = _clean_text(writer_tag.get_text(" ", strip=True)) if writer_tag else None
    published_at = _parse_published_at(
        published_at_tag.get_text(" ", strip=True)
        if published_at_tag
        else None
    )
    body = _build_body(story_tags)

    raw_payload_json = json.dumps(
        {
            "source_url": source_url,
            "content_id": content_id,
            "entry_url": entry_url,
            "page_no": page_no,
            "article_list_item": list_item_html,
            "title": str(title_tag),
            "author": str(writer_tag) if writer_tag else None,
            "published_at": str(published_at_tag) if published_at_tag else None,
            "body": [str(story_tag) for story_tag in story_tags],
        },
        ensure_ascii=False,
    )

    return RawContentResult.create(
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


def _find_body_root(soup: BeautifulSoup) -> Tag | None:
    for detail_view in soup.select("div.news_detail_view"):
        if detail_view.select_one("div.news_detail_cont") is not None:
            return detail_view

    return None


def _build_body(story_tags: list[Tag]) -> str | None:
    paragraphs = []
    for story_tag in story_tags:
        text = _clean_text(story_tag.get_text("\n", strip=True))
        if text:
            paragraphs.append(text)

    if len(paragraphs) == 0:
        return None

    return "\n\n".join(paragraphs)


def _clean_text(value: str) -> str:
    return re.sub(r"[ \t\r\f\v]+", " ", value).strip()


def _parse_published_at(value: str | None) -> datetime | None:
    if value is None or value.strip() == "":
        return None

    parsed = datetime.strptime(value.strip(), "%Y.%m.%d. %H:%M")
    return parsed.replace(tzinfo=SEOUL_TIMEZONE)


def _extract_content_id(href: str | None) -> str | None:
    if href is None:
        return None

    match = re.search(r"/archives/([^/?#]+)", href)
    if match is None:
        return None

    return match.group(1)
