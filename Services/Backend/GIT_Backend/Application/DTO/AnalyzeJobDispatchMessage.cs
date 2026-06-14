namespace GIT_Backend.Application.DTO;

public sealed record AnalyzeJobDispatchMessage(
    Guid AnalyzeJobId,
    Guid RawContentId,
    short AnalyzerProviderId,
    string PromptPolicyCode,
    string Title,
    string Body);
