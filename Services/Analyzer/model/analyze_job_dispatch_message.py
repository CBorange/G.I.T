from dataclasses import dataclass


@dataclass(slots=True, frozen=True)
class AnalyzeJobDispatchMessage:
    analyze_job_id: str
    raw_content_id: str
    analyzer_provider_id: int
    prompt_policy_code: str
    title: str
    body: str

    @staticmethod
    def from_stream_values(values: dict[str, str]) -> "AnalyzeJobDispatchMessage":
        return AnalyzeJobDispatchMessage(
            analyze_job_id=values["analyze_job_id"],
            raw_content_id=values["raw_content_id"],
            analyzer_provider_id=int(values["analyzer_provider_id"]),
            prompt_policy_code=values["prompt_policy_code"],
            title=values["title"],
            body=values["body"],
        )
