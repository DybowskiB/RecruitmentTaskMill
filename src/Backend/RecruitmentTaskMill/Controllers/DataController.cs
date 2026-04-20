using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/data")]
public sealed class DataController(IClientIdProvider clientIdProvider, IDataRequestService dataRequestService) : ControllerBase
{
    [HttpPost("request")]
    public async Task<IActionResult> CreateRequest(CancellationToken cancellationToken)
    {
        var clientId = clientIdProvider.GetOrCreateClientId(HttpContext);
        var requestId = await dataRequestService.CreateOrGetRequestAsync(clientId, cancellationToken);

        return Accepted(new
        {
            requestId,
            statusUrl = $"/api/data/request/{requestId}"
        });
    }

    [HttpGet("request/{requestId:guid}")]
    public IActionResult GetRequestStatus(Guid requestId)
    {
        var status = dataRequestService.GetStatus(requestId);
        if (status is null)
        {
            return NotFound();
        }

        return Ok(status);
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestForCurrentClient(CancellationToken cancellationToken)
    {
        var clientId = clientIdProvider.GetOrCreateClientId(HttpContext);
        var requestId = await dataRequestService.CreateOrGetRequestAsync(clientId, cancellationToken);

        var status = dataRequestService.GetStatus(requestId);
        return Ok(status);
    }
}
