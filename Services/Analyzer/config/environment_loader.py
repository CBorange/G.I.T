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
    internal_api_key: str
    redis_host: str
    redis_port: int
    redis_password: str
    redis_disaptch_stream: str
    redis_disaptch_consumer_group: str
    redis_analyzed_stream: str
    openai_api_key: str


load_dotenv(Path(__file__).resolve().parents[1] / ".env")

app_config = AppConfig(
    backend_api_base_url=require_env("BACKEND_API_BASE_URL"),
    backend_api_timeout_sec=int(
        os.getenv("BACKEND_API_TIMEOUT_SEC") or require_env("BACKEND_API_TIMEOUT_SECONDS")
    ),
    internal_api_key=require_env("Internal_API_Key"),
    openai_api_key=require_env("OpenAI_API_Key"),
    redis_host=require_env("Redis_Host"),
    redis_port=int(require_env("Redis_Port")),
    redis_password=require_env("Redis_Password"),
    redis_disaptch_stream=require_env("Redis_Disaptch_Stream"),
    redis_analyzed_stream=require_env("Redis_Analyzed_Stream"), 
    redis_disaptch_consumer_group=require_env("Redis_Disaptch_Consumer_Group"),
)
