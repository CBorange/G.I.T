namespace GIT_Backend.Domain.Entity;

public class CrawlTarget
{
    public int Id { get; set; }

    public short SourceProviderId { get; set; }

    public short SourceCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string EntryUrl { get; set; } = string.Empty;

    public int? RequestDelayMs { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? LastRunningAt { get; set; }

    public SourceProvider SourceProvider { get; set; } = null!;

    public SourceCategory SourceCategory { get; set; } = null!;

    public ICollection<RawContent> RawContents { get; } = new List<RawContent>();
}
