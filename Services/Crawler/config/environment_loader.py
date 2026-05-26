from dataclasses import dataclass
from dotenv import load_dotenv
import os


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


load_dotenv()

app_config = AppConfig(
    backend_api_base_url=require_env("BACKEND_API_BASE_URL"),
    backend_api_timeout_sec=int(
        os.getenv("BACKEND_API_TIMEOUT_SEC") or require_env("BACKEND_API_TIMEOUT_SECONDS")
    )
)
