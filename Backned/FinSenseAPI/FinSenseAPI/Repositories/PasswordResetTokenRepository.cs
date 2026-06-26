using System;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Data;
using FinSenseAPI.Models;
using FinSenseAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinSenseAPI.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _db;

    public PasswordResetTokenRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        await _db.PasswordResetTokens.AddAsync(token, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(Guid token, CancellationToken ct = default)
    {
        return await _db.PasswordResetTokens.SingleOrDefaultAsync(p => p.Token == token, ct);
    }

    public async Task MarkAsUsedAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        token.IsUsed = true;
        _db.PasswordResetTokens.Update(token);
        await _db.SaveChangesAsync(ct);
    }
}
