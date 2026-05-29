namespace GIT_Backend.Application.DTO;

public sealed record SourceProviderResponse(
    short Id,
    string Name,
    string Code,
    string BaseUrl,
    bool IsActive,
    int RequestDelayMs,
    string? Description,
    DateTimeOffset? LastRunningAt,
    IReadOnlyList<CrawlTargetResponse> CrawlTargets);

public sealed record CrawlTargetResponse(
    int Id,
    short SourceCategoryId,
    string Name,
    string Code,
    string EntryUrl,
    int? RequestDelayMs,
    bool IsActive,
    DateTimeOffset? LastRunningAt);
