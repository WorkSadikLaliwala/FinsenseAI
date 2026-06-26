using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FinSenseAPI.DTOs.EMI;
using Microsoft.AspNetCore.RateLimiting;
using FinSenseAPI.Services.Interfaces;

namespace FinSenseAPI.Controllers;

[ApiController]
[Route("api/v1/emi")]
public class EMIController : ControllerBase
{
    private readonly IEMIService _emiService;

    public EMIController(IEMIService emiService)
    {
        _emiService = emiService;
    }

    [HttpPost("check")]
    [EnableRateLimiting("claude")]
    public async Task<ActionResult<EMIResponseDto>> Check([FromBody] EMIRequestDto request)
    {
        var ct = HttpContext.RequestAborted;
        var result = await _emiService.CheckAffordabilityAsync(request, ct);
        return Ok(result);
    }
}
