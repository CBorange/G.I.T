namespace GIT_Backend.Application.DTO;

public sealed record CrawlerRawContentMessage(
    Guid Id,
    int CrawlTargetId,
    string SourceUrl,
    string? ContentId,
    string? Author,
    DateTimeOffset? PublishedAt,
    string Title,
    string? Body,
    string? RawPayloadJson,
    DateTimeOffset CrawledAt);

public sealed record RawContentSaveResult(
    Guid RawContentId,
    Guid? AnalyzeJobId,
    bool Created);
