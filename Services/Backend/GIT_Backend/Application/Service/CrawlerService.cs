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
                Id: sourceProvider.Id,
                Name: sourceProvider.Name,
                Code: sourceProvider.Code,
                BaseUrl: sourceProvider.BaseUrl,
                IsActive: sourceProvider.IsActive,
                RequestDelayMs: sourceProvider.RequestDelayMs,
                Description: sourceProvider.Description,
                LastRunningAt: sourceProvider.LastRunningAt,
                CrawlTargets: sourceProvider.CrawlTargets
                    .Where(crawlTarget => crawlTarget.IsActive)
                    .OrderBy(crawlTarget => crawlTarget.Id)
                    .Select(crawlTarget => new CrawlTargetResponse
                    (
                        Id: crawlTarget.Id,
                        SourceCategoryId: crawlTarget.SourceCategoryId,
                        Name: crawlTarget.Name,
                        Code: crawlTarget.Code,
                        EntryUrl: crawlTarget.EntryUrl,
                        RequestDelayMs: crawlTarget.RequestDelayMs,
                        IsActive: crawlTarget.IsActive,
                        LastRunningAt: crawlTarget.LastRunningAt
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);
    }
}
