using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Models;

namespace FinSenseAPI.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task BulkInsertAsync(IEnumerable<Transaction> transactions, CancellationToken ct = default);
    Task<IEnumerable<Transaction>> GetCurrentMonthBySessionAsync(Guid sessionId, int month, int year, CancellationToken ct = default);
    Task DeleteBySessionIdsAsync(IEnumerable<Guid> sessionIds, CancellationToken ct = default);
}
