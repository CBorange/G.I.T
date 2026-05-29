namespace GIT_Backend.Domain.Entity;

public class SourceProvider
{
    public short Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int RequestDelayMs { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? LastRunningAt { get; set; }

    public ICollection<CrawlTarget> CrawlTargets { get; } = new List<CrawlTarget>();

    public ICollection<AnalysisRoute> AnalysisRoutes { get; } = new List<AnalysisRoute>();
}
