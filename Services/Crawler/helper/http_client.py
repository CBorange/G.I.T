from typing import Any

import requests
from requests import Response, Session


def create_session(headers: dict[str, str] | None = None) -> Session:
    headers = {
        "User-Agent": "GIT-Crawler/1.0",
        "Accept": "application/json",
        "Content-Type": "application/json",
        "Authorization": None,
    }

    session = requests.Session()

    if headers is not None:
        session.headers.update(headers)

    return session


def http_get(session: Session, url: str, *, params: dict[str, Any] | None = None,
             headers: dict[str, str] | None = None, timeout_seconds: int = 10) -> Response:
    response = session.get(url, params=params, headers=headers, timeout=timeout_seconds)
    response.raise_for_status()
    return response


def http_post(session: Session, url: str, *, data: Any | None = None, json: Any | None = None,
              headers: dict[str, str] | None = None, timeout_seconds: int = 10) -> Response:
    response = session.post(url, data=data, json=json, headers=headers, timeout=timeout_seconds)
    response.raise_for_status()
    return response


def http_put(session: Session, url: str, *, data: Any | None = None, json: Any | None = None,
             headers: dict[str, str] | None = None, timeout_seconds: int = 10) -> Response:
    response = session.put(url, data=data, json=json, headers=headers, timeout=timeout_seconds)
    response.raise_for_status()
    return response


def http_delete(session: Session, url: str, *, headers: dict[str, str] | None = None,
                timeout_seconds: int = 10) -> Response:
    response = session.delete(url, headers=headers, timeout=timeout_seconds)
    response.raise_for_status()
    return response