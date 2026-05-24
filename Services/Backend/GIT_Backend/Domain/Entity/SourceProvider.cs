namespace GIT_Backend.Domain.Entity;

public class SourceProvider
{
    public short Id { get; set; }

    public short ExpectCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int IntervalMin { get; set; }

    public int RequestDelayMs { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public SourceCategory ExpectCategory { get; set; } = null!;

    public ICollection<RawContent> RawContents { get; } = new List<RawContent>();

    public ICollection<AnalysisRoute> AnalysisRoutes { get; } = new List<AnalysisRoute>();
}
