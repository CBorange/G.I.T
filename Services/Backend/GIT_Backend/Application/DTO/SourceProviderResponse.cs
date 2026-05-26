namespace GIT_Backend.Application.DTO;

public sealed record SourceProviderResponse(
    string Name,
    string Code,
    string BaseUrl,
    int IntervalMin,
    int RequestDelayMs);
