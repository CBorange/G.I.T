from dataclasses import dataclass
from dotenv import load_dotenv
import os
from pathlib import Path


def require_env(name: str) -> str:
    value = os.getenv(name)

    if value is None or value.strip() == "":
        raise RuntimeError(
            f"Required environment variable is missing: {name}"
        )
    return value


@dataclass(slots=True, frozen=True)
class AppConfig:
    backend_api_base_url: str
    backend_api_timeout_sec: int
    redis_host: str
    redis_port: int
    redis_password: str
    raw_content_stream_key: str


load_dotenv(Path(__file__).resolve().parents[1] / ".env")

app_config = AppConfig(
    backend_api_base_url=require_env("BACKEND_API_BASE_URL"),
    backend_api_timeout_sec=int(
        os.getenv("BACKEND_API_TIMEOUT_SEC") or require_env("BACKEND_API_TIMEOUT_SECONDS")
    ),
    redis_host=require_env("Redis_Host"),
    redis_port=int(require_env("Redis_Port")),
    redis_password=require_env("Redis_Password"),
    raw_content_stream_key=os.getenv("RAW_CONTENT_STREAM_KEY") or "raw-content-events",
)
