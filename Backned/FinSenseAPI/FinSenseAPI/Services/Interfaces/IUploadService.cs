using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FinSenseAPI.DTOs.Upload;

namespace FinSenseAPI.Services.Interfaces;

public interface IUploadService
{
    Task<UploadResponseDto> ProcessUploadAsync(IFormFile file, Guid? userId, CancellationToken ct);
    Task<System.Collections.Generic.IEnumerable<FinSenseAPI.DTOs.Upload.UploadHistoryItemDto>> GetHistoryAsync(Guid userId, CancellationToken ct);
}
