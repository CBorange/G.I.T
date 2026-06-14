namespace GIT_Backend.Application.DTO;

public sealed record SourceProviderResponse(
    short Id,
    string Code,
    string BaseUrl,
    int RequestDelayMs,
    IReadOnlyList<CrawlTargetResponse> CrawlTargets);

public sealed record CrawlTargetResponse(
    int Id,
    string Code,
    string EntryUrl,
    int? RequestDelayMs);
