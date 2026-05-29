namespace GIT_Backend.Domain.Entity;

public class AnalyzedContent
{
    public Guid Id { get; set; }

    public Guid RawContentId { get; set; }

    public short AnalyzerProviderId { get; set; }

    public Guid AnalyzeJobId { get; set; }

    public short ActualCategoryId { get; set; }

    public string TitleSummary { get; set; } = string.Empty;

    public string BodySummary { get; set; } = string.Empty;

    public string? KeywordJson { get; set; }

    public string? LocationJson { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string? AnalysisPayloadJson { get; set; }

    public DateTimeOffset AnalyzedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public decimal Confidence { get; set; }

    public string ConfidenceReason { get; set; } = string.Empty;

    public RawContent RawContent { get; set; } = null!;

    public AnalyzeJob AnalyzeJob { get; set; } = null!;

    public AnalyzerProvider AnalyzerProvider { get; set; } = null!;

    public SourceCategory ActualCategory { get; set; } = null!;
}
