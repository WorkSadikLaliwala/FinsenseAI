using System;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.Data;
using FinSenseAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FinSenseAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _db.Users.SingleOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Users.FindAsync(new object[] { id }, ct);
    }

    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _db.Users.AnyAsync(u => u.Email == email, ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return await _db.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);
    }

    public async Task DeleteAllUserDataAsync(Guid userId, CancellationToken ct = default)
    {
        // Delete chat messages for user's sessions
        var sessionIds = await _db.UploadSessions.Where(s => s.UserId == userId).Select(s => s.Id).ToListAsync(ct);
        if (sessionIds.Any())
        {
            var chats = await _db.ChatMessages.Where(c => sessionIds.Contains(c.SessionId)).ToListAsync(ct);
            if (chats.Any()) _db.ChatMessages.RemoveRange(chats);

            var txs = await _db.Transactions.Where(t => sessionIds.Contains(t.SessionId)).ToListAsync(ct);
            if (txs.Any()) _db.Transactions.RemoveRange(txs);

            var uploads = await _db.UploadSessions.Where(s => s.UserId == userId).ToListAsync(ct);
            if (uploads.Any()) _db.UploadSessions.RemoveRange(uploads);
        }

        // Delete goals
        var goals = await _db.Goals.Where(g => g.UserId == userId).ToListAsync(ct);
        if (goals.Any()) _db.Goals.RemoveRange(goals);

        await _db.SaveChangesAsync(ct);
    }
}
