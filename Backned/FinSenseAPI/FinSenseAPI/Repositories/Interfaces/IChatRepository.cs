using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Models;

namespace FinSenseAPI.Repositories.Interfaces;

public interface IChatRepository
{
    Task SaveMessageAsync(ChatMessage message, CancellationToken ct = default);
    Task<IEnumerable<ChatMessage>> GetBySessionIdAsync(Guid sessionId, int limit = 20, CancellationToken ct = default);
    Task DeleteBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
}
