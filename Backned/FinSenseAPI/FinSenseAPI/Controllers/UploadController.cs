using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using FinSenseAPI.DTOs.Upload;
using FinSenseAPI.Services.Interfaces;

namespace FinSenseAPI.Controllers;

[ApiController]
[Route("api/v1/upload")]
public class UploadController : ControllerBase
{
    private readonly IUploadService _uploadService;

    public UploadController(IUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadResponseDto>> Upload([FromForm] UploadRequestDto request)
    {
        var userId = GetUserId();
        // Basic validation: file type and size
        const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
        var file = request?.File;
        if (file == null) return BadRequest("No file uploaded.");
        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only CSV files are accepted.");
        if (file.Length > MaxFileSizeBytes)
            return BadRequest($"File size must not exceed {MaxFileSizeBytes / (1024 * 1024)} MB.");
        var ct = HttpContext.RequestAborted;
        var response = await _uploadService.ProcessUploadAsync(file, userId == Guid.Empty ? (Guid?)null : userId, ct);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<ActionResult<System.Collections.Generic.IEnumerable<UploadHistoryItemDto>>> GetHistory()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var ct = HttpContext.RequestAborted;
        var list = await _uploadService.GetHistoryAsync(userId, ct);
        return Ok(list);
    }

    private Guid GetUserId()
    {
        // Try standard ClaimTypes first
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value
               ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
