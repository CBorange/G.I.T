namespace GIT_Backend.Domain.Entity;

public class AnalysisRoute
{
    public short Id { get; set; }

    public short SourceProviderId { get; set; }

    public short AnalyzerProviderId { get; set; }

    public bool IsEnabled { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public SourceProvider SourceProvider { get; set; } = null!;

    public AnalyzerProvider AnalyzerProvider { get; set; } = null!;
}
