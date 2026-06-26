using System;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.DTOs.Chat;

namespace FinSenseAPI.Services.Interfaces;

public interface IChatService
{
    Task<ChatResponseDto> SendMessageAsync(ChatRequestDto dto, bool isGuest, CancellationToken ct);
    Task DeleteHistoryAsync(Guid sessionId, Guid userId, CancellationToken ct);
}
