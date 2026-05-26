from bs4 import BeautifulSoup

from models.source_provider import Article


def parse_articles(html: str) -> list[Article]:
    soup = BeautifulSoup(html, "html.parser")

    articles: list[Article] = []

    # TODO:
    # 1. 기사 목록 selector 지정
    # 2. title/url 추출
    # 3. 필수값 없는 항목 skip
    # 4. 상대경로 URL 보정

    return articles