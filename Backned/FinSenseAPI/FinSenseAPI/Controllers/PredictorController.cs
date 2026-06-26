using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinSenseAPI.DTOs.Predictor;
using Microsoft.AspNetCore.RateLimiting;
using FinSenseAPI.Services.Interfaces;

namespace FinSenseAPI.Controllers;

[ApiController]
[Route("api/v1/predictor")]
public class PredictorController : ControllerBase
{
    private readonly IPredictorService _predictorService;

    public PredictorController(IPredictorService predictorService)
    {
        _predictorService = predictorService;
    }

    [Authorize]
    [HttpPost("month-end")]
    [EnableRateLimiting("claude")]
    public async Task<ActionResult<PredictorResponseDto>> MonthEnd([FromBody] PredictorRequestDto request)
    {
        var ct = HttpContext.RequestAborted;
        var result = await _predictorService.PredictMonthEndAsync(request, ct);
        return Ok(result);
    }
}
