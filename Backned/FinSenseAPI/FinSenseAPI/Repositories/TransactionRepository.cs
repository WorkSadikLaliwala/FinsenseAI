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

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _db;

    public TransactionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Transaction>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        return await _db.Transactions.Where(t => t.SessionId == sessionId).ToListAsync(ct);
    }

    public async Task BulkInsertAsync(IEnumerable<Transaction> transactions, CancellationToken ct = default)
    {
        await _db.Transactions.AddRangeAsync(transactions, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Transaction>> GetCurrentMonthBySessionAsync(Guid sessionId, int month, int year, CancellationToken ct = default)
    {
        return await _db.Transactions
            .Where(t => t.SessionId == sessionId && t.Date.Month == month && t.Date.Year == year)
            .ToListAsync(ct);
    }

    public async Task DeleteBySessionIdsAsync(IEnumerable<Guid> sessionIds, CancellationToken ct = default)
    {
        var txs = await _db.Transactions.Where(t => sessionIds.Contains(t.SessionId)).ToListAsync(ct);
        if (txs.Any())
        {
            _db.Transactions.RemoveRange(txs);
            await _db.SaveChangesAsync(ct);
        }
    }
}
