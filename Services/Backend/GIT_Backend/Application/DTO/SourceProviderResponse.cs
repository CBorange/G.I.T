namespace GIT_Backend.Application.DTO;

public sealed record SourceProviderResponse(
    short Id,
    short ExpectCategoryId,
    string Name,
    string Code,
    string BaseUrl,
    string CrawlUrl,
    bool IsActive,
    int IntervalMin,
    int RateLimitMs,
    string? Description);
