namespace GIT_Backend.Domain.Entity;

public class AnalyzerProvider
{
    public short Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;

    public string? EndpointUrl { get; set; }

    public bool IsEnabled { get; set; }

    public string? ConfigJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? LastRunningAt { get; set; }

    public ICollection<AnalysisRoute> AnalysisRoutes { get; } = new List<AnalysisRoute>();

    public ICollection<AnalyzedContent> AnalyzedContents { get; } = new List<AnalyzedContent>();

    public ICollection<AnalyzeJob> AnalyzeJobs { get; } = new List<AnalyzeJob>();
}
