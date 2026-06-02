namespace GIT_Backend.Domain.Entity;

public class RawContent
{
    public Guid Id { get; set; }

    public int CrawlTargetId { get; set; }

    public string SourceUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;

    public string? ContentId { get; set; }

    public string? Author { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public string? RawPayloadJson { get; set; }

    public DateTimeOffset CrawledAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public CrawlTarget CrawlTarget { get; set; } = null!;

    public AnalyzeJob AnalyzeJob { get; set; } = null!;

    public AnalyzedContent AnalyzedContent { get; set; } = null!;
}
