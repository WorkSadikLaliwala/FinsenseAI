using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.Data;
using FinSenseAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FinSenseAPI.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _db;

    public ChatRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task SaveMessageAsync(ChatMessage message, CancellationToken ct = default)
    {
        await _db.ChatMessages.AddAsync(message, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<ChatMessage>> GetBySessionIdAsync(Guid sessionId, int limit = 20, CancellationToken ct = default)
    {
        return await _db.ChatMessages
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.Timestamp)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task DeleteBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        var messages = await _db.ChatMessages.Where(c => c.SessionId == sessionId).ToListAsync(ct);
        if (messages.Any())
        {
            _db.ChatMessages.RemoveRange(messages);
            await _db.SaveChangesAsync(ct);
        }
    }
}
