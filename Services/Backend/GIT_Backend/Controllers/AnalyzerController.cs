using GIT_Backend.Application.DTO;
using GIT_Backend.Application.Service;
using Microsoft.AspNetCore.Mvc;

namespace GIT_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyzerController(AnalyzerService analyzerService) : ControllerBase
{
    [HttpGet("analyzer-providers")]
    [ProducesResponseType(typeof(List<AnalyzerProviderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AnalyzerProviderResponse>>> GetAnalyzerProviders(
        [FromQuery(Name = "ids")] List<short> analyzerProviderIds,
        CancellationToken cancellationToken)
    {
        var analyzerProviders = await analyzerService.GetAnalyzerProvidersAsync(analyzerProviderIds, cancellationToken);

        return Ok(analyzerProviders);
    }
}
