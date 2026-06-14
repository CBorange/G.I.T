namespace GIT_Backend.Application.DTO;

public sealed record SourceCategoryResponse(
    short Id,
    string Code,
    string Name,
    string? Description);
