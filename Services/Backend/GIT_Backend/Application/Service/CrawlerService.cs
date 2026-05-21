using GIT_Backend.Application.DTO;
using GIT_Backend.Infra.Database;
using Microsoft.EntityFrameworkCore;

namespace GIT_Backend.Application.Service;

public class CrawlerService(GITDBContext dbContext)
{
    public async Task<IReadOnlyList<SourceProviderResponse>> GetActiveSourceProvidersAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SourceProviders
            .AsNoTracking()
            .Where(sourceProvider => sourceProvider.IsActive)
            .OrderBy(sourceProvider => sourceProvider.Id)
            .Select(sourceProvider => new SourceProviderResponse
            {
                Id = sourceProvider.Id,
                ExpectCategoryId = sourceProvider.ExpectCategoryId,
                Name = sourceProvider.Name,
                Code = sourceProvider.Code,
                BaseUrl = sourceProvider.BaseUrl,
                CrawlUrl = sourceProvider.CrawlUrl,
                IsActive = sourceProvider.IsActive,
                IntervalMin = sourceProvider.IntervalMin,
                RateLimitMs = sourceProvider.RateLimitMs,
                Description = sourceProvider.Description
            })
            .ToListAsync(cancellationToken);
    }
}
