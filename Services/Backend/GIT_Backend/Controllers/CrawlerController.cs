using GIT_Backend.Application.DTO;
using GIT_Backend.Application.Service;
using Microsoft.AspNetCore.Mvc;

namespace GIT_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrawlerController(CrawlerService crawlerService) : ControllerBase
{
    [HttpGet("source-providers")]
    [ProducesResponseType(typeof(List<SourceProviderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SourceProviderResponse>>> GetSourceProviders(CancellationToken cancellationToken)
    {
        var sourceProviders = await crawlerService.GetActiveSourceProvidersAsync(cancellationToken);

        return Ok(sourceProviders);
    }
}
