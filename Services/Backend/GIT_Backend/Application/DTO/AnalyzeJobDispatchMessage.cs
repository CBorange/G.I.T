namespace GIT_Backend.Application.DTO;

public sealed record AnalyzeJobDispatchMessage(
    Guid AnalyzeJobId,
    Guid RawContentId,
    short AnalyzerProviderId,
    string AnalyzerProviderCode,
    string AnalyzerModelName,
    string? AnalyzerEndpointUrl,
    string? AnalyzerConfigJson,
    string PromptPolicyCode,
    short AttemptCount,
    short MaxAttemptCount,
    int CrawlTargetId,
    string SourceUrl,
    string? ContentId,
    string? Author,
    DateTimeOffset? PublishedAt,
    string Title,
    string? Body,
    string? RawPayloadJson,
    DateTimeOffset CrawledAt);
