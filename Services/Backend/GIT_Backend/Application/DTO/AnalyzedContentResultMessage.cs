namespace GIT_Backend.Application.DTO;

public sealed record AnalyzedContentResultMessage(
    Guid Id,
    Guid RawContentId,
    short AnalyzerProviderId,
    Guid AnalyzeJobId,
    short ActualCategoryId,
    string TitleSummary,
    string BodySummary,
    string? KeywordJson,
    string? LocationJson,
    string ModelName,
    string? AnalysisPayloadJson,
    DateTimeOffset AnalyzedAt,
    decimal Confidence,
    string ConfidenceReason);

public sealed record AnalyzedContentProcessResult(
    Guid AnalyzeJobId,
    Guid? AnalyzedContentId,
    bool Succeeded,
    bool JobUpdated,
    string? FailureReason);
