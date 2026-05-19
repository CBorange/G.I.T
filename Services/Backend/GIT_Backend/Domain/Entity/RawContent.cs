namespace GIT_Backend.Domain.Entity;

public class RawContent
{
    public Guid Id { get; set; }

    public short SourceProviderId { get; set; }

    public short ExpectCategoryId { get; set; }

    public string SourceUrl { get; set; } = string.Empty;

    public string? Author { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public string? RawPayloadJson { get; set; }

    public DateTimeOffset CrawledAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public SourceProvider SourceProvider { get; set; } = null!;

    public SourceCategory ExpectCategory { get; set; } = null!;

    public AnalyzedContent? AnalyzedContent { get; set; }
}
