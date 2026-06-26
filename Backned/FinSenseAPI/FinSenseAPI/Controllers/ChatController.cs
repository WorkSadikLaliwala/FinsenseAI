using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinSenseAPI.DTOs.Chat;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;

namespace FinSenseAPI.Controllers;

[ApiController]
[Route("api/v1/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IUploadService _uploadService;

    public ChatController(IChatService chatService, IUploadService uploadService)
    {
        _chatService = chatService;
        _uploadService = uploadService;
    }

    [HttpPost("message")]
    [EnableRateLimiting("claude")]
    public async Task<ActionResult<ChatResponseDto>> SendMessage([FromBody] ChatRequestDto request)
    {
        var ct = HttpContext.RequestAborted;

        var isGuest = !User.Identity?.IsAuthenticated ?? true;

        var response = await _chatService.SendMessageAsync(request, isGuest, ct);
        return Ok(response);
    }

    [Authorize]
    [HttpDelete("history/{sessionId:guid}")]
    public async Task<IActionResult> DeleteHistory(Guid sessionId)
    {
        var ct = HttpContext.RequestAborted;
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        
        await _chatService.DeleteHistoryAsync(sessionId, userId, ct);
        //await _chatRepo.DeleteBySessionIdAsync(sessionId);
        return Ok(new { message = "Chat history cleared" });
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
