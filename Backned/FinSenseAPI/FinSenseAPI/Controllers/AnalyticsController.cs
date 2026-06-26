using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FinSenseAPI.DTOs.Analytics;
using FinSenseAPI.Services.Interfaces;

namespace FinSenseAPI.Controllers;

[ApiController]
[Route("api/v1/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }
    [HttpGet("{sessionId:guid}")]
    public async Task<ActionResult<SpendingSummaryDto>> GetAnalytics(Guid sessionId)
    {
        var ct = HttpContext.RequestAborted;
        var result = await _analyticsService.GetSummaryAsync(sessionId, ct);
        return Ok(result);
    }
}
