from redis import Redis

from config.environment_loader import app_config


def create_redis_client() -> Redis:
    return Redis(
        host=app_config.redis_host,
        port=app_config.redis_port,
        password=app_config.redis_password,
        decode_responses=True,
    )
