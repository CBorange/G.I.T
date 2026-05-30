from typing import Any

import requests
from requests import Response, Session



def create_session(headers: dict[str, str] | None = None) -> Session:
    default_headers = {
        "User-Agent": "GIT-Crawler/1.0",
        "Accept": "application/json",
        "Content-Type": "application/json",
    }

    # 사용자 입력 headers가 있으면 기본값 덮어쓰기
    merged_headers = {**default_headers, **(headers or {})}

    session = requests.Session()
    session.headers.update(merged_headers)

    return session

def create_crawler_session() -> Session:
    return create_session(
        headers={
            "User-Agent": (
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
                "AppleWebKit/537.36 (KHTML, like Gecko) "
                "Chrome/136.0.0.0 Safari/537.36"
            ),
            "Accept": (
                "text/html,"
                "application/xhtml+xml,"
                "application/xml;q=0.9,"
                "image/avif,"
                "image/webp,"
                "*/*;q=0.8"
            ),
            "Accept-Language": "ko-KR,ko;q=0.9,en;q=0.8",
        }
    )
    