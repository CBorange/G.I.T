namespace GIT_Backend.Domain.Entity;

public class AnalyzeJob
{
    public Guid Id { get; set; }

    public Guid RawContentId { get; set; }

    public short AnalyzerProviderId { get; set; }

    public string PromptPolicyCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public short AttemptCount { get; set; }

    public short? MaxAttemptCount { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset? LastRunningAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public RawContent RawContent { get; set; } = null!;

    public AnalyzerProvider AnalyzerProvider { get; set; } = null!;

    public AnalyzedContent AnalyzedContent { get; set; } = null!;
}
