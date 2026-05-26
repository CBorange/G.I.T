using GIT_Backend.Application.DTO;
using GIT_Backend.Infra.Database;
using Microsoft.EntityFrameworkCore;

namespace GIT_Backend.Application.Service;

public class CrawlerService(GITDBContext dbContext)
{
    public async Task<IReadOnlyList<SourceProviderResponse>> GetActiveProvidersAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SourceProviders
            .AsNoTracking()
            .Where(sourceProvider => sourceProvider.IsActive)
            .OrderBy(sourceProvider => sourceProvider.Id)
            .Select(sourceProvider => new SourceProviderResponse
            (
                Name: sourceProvider.Name,
                Code: sourceProvider.Code,
                BaseUrl: sourceProvider.BaseUrl,
                IntervalMin: sourceProvider.IntervalMin,
                RequestDelayMs: sourceProvider.RequestDelayMs
            ))
            .ToListAsync(cancellationToken);
    }
}
