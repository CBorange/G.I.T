namespace GIT_Backend.Domain.Entity;

public class AnalysisRoute
{
    public short Id { get; set; }

    public short? SourceProviderId { get; set; }

    public short? SourceCategoryId { get; set; }

    public short AnalyzerProviderId { get; set; }

    public string PromptPolicyCode { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public SourceProvider? SourceProvider { get; set; }

    public SourceCategory? SourceCategory { get; set; }

    public AnalyzerProvider AnalyzerProvider { get; set; } = null!;
}
