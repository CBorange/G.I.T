import argparse
import logging

mode = "loop"
provider: str | None = None


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
    args = parser.parse_args()

    global mode, provider
    mode = args.mode
    provider = args.provider
    if mode == "once" and provider is None:
        parser.error("--provider is required when --mode once")

    logging.info(f"====== Crawler Service Mode: {mode} =======")
    return args
