using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinSenseAPI.Services.Interfaces;
using FinSenseAPI.DTOs.User;

namespace FinSenseAPI.Controllers;

[ApiController]
[Route("api/v1/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var profile = await _userService.GetProfileAsync(userId, HttpContext.RequestAborted);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequestDto request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var updated = await _userService.UpdateProfileAsync(userId, request, HttpContext.RequestAborted);
        if (updated == null) return BadRequest(new { message = "Current password is incorrect or user not found" });
        return Ok(new { fullName = updated.FullName, email = updated.Email });
    }

    [Authorize]
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequestDto request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var ok = await _userService.DeleteAccountAsync(userId, request, HttpContext.RequestAborted);
        if (!ok) return BadRequest(new { message = "Password is incorrect or user not found" });
        return Ok(new { message = "Account deleted. All data removed." });
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
