using GIT_Backend.Application.Common;
using GIT_Backend.Application.DTO;
using GIT_Backend.Application.Worker;
using GIT_Backend.Domain.Constants;
using GIT_Backend.Domain.Entity;
using GIT_Backend.Infra.Database;
using Microsoft.EntityFrameworkCore;

namespace GIT_Backend.Application.Service;

public class CrawlerService
{
    private readonly GITDBContext _dbContext;
    private readonly ILogger<CrawlerService> _logger;

    public CrawlerService(ILogger<CrawlerService> logger, GITDBContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    public async Task<IReadOnlyList<SourceProviderResponse>> GetActiveProvidersAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SourceProviders
            .AsNoTracking()
            .Where(sourceProvider => sourceProvider.IsActive)
            .OrderBy(sourceProvider => sourceProvider.Id)
            .Select(sourceProvider => new SourceProviderResponse
            (
                Id: sourceProvider.Id,
                Code: sourceProvider.Code,
                BaseUrl: sourceProvider.BaseUrl,
                RequestDelayMs: sourceProvider.RequestDelayMs,
                CrawlTargets: sourceProvider.CrawlTargets
                    .Where(crawlTarget => crawlTarget.IsActive)
                    .OrderBy(crawlTarget => crawlTarget.Id)
                    .Select(crawlTarget => new CrawlTargetResponse
                    (
                        Id: crawlTarget.Id,
                        Code: crawlTarget.Code,
                        EntryUrl: crawlTarget.EntryUrl,
                        RequestDelayMs: crawlTarget.RequestDelayMs
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<RawContentSaveResult> SaveRawContentAsync(CrawlerRawContentMessage message, CancellationToken cancellationToken)
    {
        var existingRawContentId = await FindExistingRawContentIdAsync(message, cancellationToken);
        if (existingRawContentId is not null)
        {
            return new RawContentSaveResult(existingRawContentId.Value, AnalyzeJobId: null, Created: false);
        }

        var crawlTarget = await _dbContext.CrawlTargets
            .AsNoTracking()
            .Where(crawlTarget => crawlTarget.Id == message.CrawlTargetId)
            .Select(crawlTarget => new
            {
                crawlTarget.Id,
                crawlTarget.SourceProviderId,
                crawlTarget.SourceCategoryId,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (crawlTarget is null)
        {
            throw new NotFoundException("CrawlTarget", message.CrawlTargetId);
        }

        // AnalysisRoute 테이블에서 RawContents 발행한 CrawlTarget과 연결되는 Route가 있는지 검증한다.
        // AnalyzeJob 데이터 생성 -> AI 분석 요청과 도메인적으로 같음, Redis Event 발행과 별개로 AnalyzeJob이 생성되어 있으면 분석 예약인것이므로
        // 이 단계에서 AnalysisRoute를 Check 하는것
        var analysisRoute = await _dbContext.AnalysisRoutes
            .AsNoTracking()
            .Where(route =>
                route.IsEnabled &&
                route.AnalyzerProvider.IsEnabled &&
                (
                    (route.SourceProviderId == crawlTarget.SourceProviderId &&
                        route.SourceCategoryId == crawlTarget.SourceCategoryId)
                    ||
                    route.SourceProviderId == crawlTarget.SourceProviderId
                    ||
                    route.SourceCategoryId == crawlTarget.SourceCategoryId
                    ||
                    route.IsDefault
                ))
            .OrderByDescending(route =>
                route.SourceProviderId == crawlTarget.SourceProviderId &&
                route.SourceCategoryId == crawlTarget.SourceCategoryId)
            .ThenByDescending(route => route.SourceProviderId == crawlTarget.SourceProviderId)
            .ThenByDescending(route => route.SourceCategoryId == crawlTarget.SourceCategoryId)
            .ThenByDescending(route => route.IsDefault)
            .ThenBy(route => route.Id)
            .Select(route => new
            {
                route.AnalyzerProviderId,
                route.PromptPolicyCode,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (analysisRoute is null)
        {
            string msg = "AnalysisRoute discovery is the process of searching for matching route records based on the route policy. " +
                "This error does not occur if there is no defined key and at least a default route exists." +
                " Please check the DB tables.";
            throw new NotFoundException("AnalysisRoute", "", msg);
        }

        var rawContent = new RawContent
        {
            Id = message.Id,
            CrawlTargetId = message.CrawlTargetId,
            SourceUrl = message.SourceUrl,
            ContentId = message.ContentId,
            Author = message.Author,
            PublishedAt = message.PublishedAt?.ToUniversalTime(),
            Title = message.Title,
            Body = message.Body,
            RawPayloadJson = message.RawPayloadJson,
            CrawledAt = message.CrawledAt.ToUniversalTime(),
        };

        var analyzeJob = new AnalyzeJob
        {
            Id = Guid.NewGuid(),
            RawContentId = rawContent.Id,
            AnalyzerProviderId = analysisRoute.AnalyzerProviderId,
            PromptPolicyCode = analysisRoute.PromptPolicyCode,
            Status = AnalyzeJobStatus.Pending.ToString(),
            AttemptCount = 0,
            MaxAttemptCount = null,
        };

        try
        {
            _dbContext.RawContents.Add(rawContent);
            _dbContext.AnalyzeJobs.Add(analyzeJob);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save RawContent and AnalyzeJob CrawlTargetId={CrawlTargetId}", message.CrawlTargetId);
            throw;
        }

        return new RawContentSaveResult(rawContent.Id, analyzeJob.Id, Created: true);
    }

    private async Task<Guid?> FindExistingRawContentIdAsync(
        CrawlerRawContentMessage message,
        CancellationToken cancellationToken)
    {
        return await _dbContext.RawContents
            .Where(rawContent =>
                rawContent.Id == message.Id ||
                rawContent.SourceUrl == message.SourceUrl ||
                (message.ContentId != null && rawContent.ContentId == message.ContentId))
            .Select(rawContent => (Guid?)rawContent.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
