namespace GIT_Backend.Application.DTO;

public sealed record AnalyzerProviderResponse(
    short Id,
    string Name,
    string Code,
    string ModelName,
    string? EndpointUrl,
    bool IsEnabled,
    string? ConfigJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastRunningAt);
