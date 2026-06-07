import requests
from requests import Session


def create_session(headers: dict[str, str] | None = None) -> Session:
    default_headers = {
        "User-Agent": "GIT-Analyzer/1.0",
        "Accept": "application/json",
        "Content-Type": "application/json",
    }

    # 사용자 입력 headers가 있으면 기본값 덮어쓰기
    merged_headers = {**default_headers, **(headers or {})}

    session = requests.Session()
    session.headers.update(merged_headers)

    return session
