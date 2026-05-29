namespace GIT_Backend.Domain.Entity;

public class SourceCategory
{
    public short Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<CrawlTarget> CrawlTargets { get; } = new List<CrawlTarget>();

    public ICollection<AnalyzedContent> AnalyzedContents { get; } = new List<AnalyzedContent>();

    public ICollection<AnalysisRoute> AnalysisRoutes { get; } = new List<AnalysisRoute>();
}
