namespace GIT_Backend.Domain.Entity;

public class SourceCategory
{
    public short Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<SourceProvider> SourceProviders { get; } = new List<SourceProvider>();

    public ICollection<RawContent> RawContents { get; } = new List<RawContent>();

    public ICollection<AnalyzedContent> AnalyzedContents { get; } = new List<AnalyzedContent>();
}
