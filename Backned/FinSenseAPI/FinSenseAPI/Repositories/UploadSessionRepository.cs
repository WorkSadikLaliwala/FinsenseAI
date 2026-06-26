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

public class UploadSessionRepository : IUploadSessionRepository
{
    private readonly AppDbContext _db;

    public UploadSessionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(UploadSession session, CancellationToken ct = default)
    {
        await _db.UploadSessions.AddAsync(session, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<UploadSession?> GetByHashAndUserAsync(string hash, Guid? userId, CancellationToken ct = default)
    {
        return await _db.UploadSessions.Include(s => s.Transactions)
            .SingleOrDefaultAsync(s => s.FileHash == hash && s.UserId == userId, ct);
    }

    public async Task<UploadSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.UploadSessions.Include(s => s.Transactions).SingleOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IEnumerable<UploadSession>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.UploadSessions.Include(s => s.Transactions).Where(s => s.UserId == userId).ToListAsync(ct);
    }

    public async Task DeleteExpiredGuestSessionsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var expired = await _db.UploadSessions.Where(s => s.IsGuest && s.ExpiresAt != null && s.ExpiresAt < now).ToListAsync(ct);
        if (expired.Any())
        {
            _db.UploadSessions.RemoveRange(expired);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var sessions = await _db.UploadSessions.Where(s => s.UserId == userId).ToListAsync(ct);
        if (sessions.Any())
        {
            // remove related transactions and chat messages via cascade or explicitly
            _db.UploadSessions.RemoveRange(sessions);
            await _db.SaveChangesAsync(ct);
        }
    }
}
