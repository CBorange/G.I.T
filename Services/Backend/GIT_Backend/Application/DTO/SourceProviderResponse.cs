namespace GIT_Backend.Application.DTO;

public sealed class SourceProviderResponse
{
    public short Id { get; init; }

    public short ExpectCategoryId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;

    public string CrawlUrl { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public int IntervalMin { get; init; }

    public int RateLimitMs { get; init; }

    public string? Description { get; init; }
}
