using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinSenseAPI.DTOs.Goal;
using FinSenseAPI.Services.Interfaces;
using System;

namespace FinSenseAPI.Controllers;

[ApiController]
[Route("api/v1/goals")]
public class GoalController : ControllerBase
{
    private readonly IGoalService _goalService;

    public GoalController(IGoalService goalService)
    {
        _goalService = goalService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<GoalResponseDto>> Create([FromBody] GoalRequestDto request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var ct = HttpContext.RequestAborted;
        var created = await _goalService.EvaluateGoalAsync(request, userId, ct);
        return Ok(created);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GoalDto>>> GetAll()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var ct = HttpContext.RequestAborted;
        var list = await _goalService.GetUserGoalsAsync(userId, ct);
        return Ok(list);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var ct = HttpContext.RequestAborted;
        var ok = await _goalService.DeleteGoalAsync(id, userId, ct);
        if (!ok) return NotFound(new { message = "Goal not found or not allowed" });
        return Ok(new { message = "Goal deleted" });
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<GoalDto>> Update(int id, [FromBody] UpdateGoalRequestDto request)
    {
        var userId = GetUserId();
        var ct = HttpContext.RequestAborted;
        try
        {
            var updated = await _goalService.UpdateGoalAsync(id, request, userId, ct);
            return Ok(updated);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Goal not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You do not have permission to update this goal");
        }
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
