using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinSenseAPI.Services.Interfaces;
using FinSenseAPI.DTOs.Analytics;
using FinSenseAPI.Repositories.Interfaces;

namespace FinSenseAPI.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ITransactionRepository _txRepo;

    public AnalyticsService(ITransactionRepository txRepo)
    {
        _txRepo = txRepo;
    }

    //public async Task<SpendingSummaryDto> GetSummaryAsync(Guid sessionId, CancellationToken ct)
    //{
    //    var txs = (await _txRepo.GetBySessionIdAsync(sessionId, ct)).ToList();

    //    var totalIncome = txs.Where(t => string.Equals(t.Type, "Credit", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
    //    var totalSpending = txs.Where(t => !string.Equals(t.Type, "Credit", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
    //    var totalRefunds = txs.Where(t => string.Equals(t.Type, "Refund", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
    //    var netSavings = totalIncome - totalSpending;

    //    var byCategory = txs
    //        .Where(t => !string.IsNullOrEmpty(t.Category))
    //        .GroupBy(t => t.Category)
    //        .Select(g => new CategoryBreakdownDto { Category = g.Key, Amount = g.Sum(t => t.Amount) })
    //        .OrderByDescending(c => c.Amount)
    //        .ToList();

    //    var topTx = txs.OrderByDescending(t => Math.Abs(t.Amount)).Take(5)
    //        .Select(t => new FinSenseAPI.DTOs.Upload.TransactionDto { Date = t.Date, Description = t.Description, Amount = t.Amount, Category = t.Category })
    //        .ToList();

    //    var summary = new SpendingSummaryDto
    //    {
    //        TotalIncome = totalIncome,
    //        TotalRefunds = totalRefunds,
    //        TotalSpending = totalSpending,
    //        NetSavings = netSavings,
    //        ByCategory = byCategory,
    //        TopTransactions = topTx
    //    };

    //    return summary;
    //}
    public async Task<SpendingSummaryDto> GetSummaryAsync(Guid sessionId, CancellationToken ct)
    {
        var txs = (await _txRepo.GetBySessionIdAsync(sessionId, ct)).ToList();

        var totalRefunds = txs.Where(t => t.Description != null && t.Description.Contains("refund", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
        var totalIncome = txs.Where(t => string.Equals(t.Type, "Credit", StringComparison.OrdinalIgnoreCase) && !(t.Description != null && t.Description.Contains("refund", StringComparison.OrdinalIgnoreCase))).Sum(t => t.Amount);
        var totalSpending = txs.Where(t => string.Equals(t.Type, "Debit", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
        var netSavings = totalIncome + totalRefunds - totalSpending;

        // Bug 1 fix — Debit only for category breakdown
        var debits = txs.Where(t => string.Equals(t.Type, "Debit", StringComparison.OrdinalIgnoreCase)).ToList();
        var denom = totalSpending > 0 ? totalSpending : 1;

        var byCategory = debits
            .Where(t => !string.IsNullOrEmpty(t.Category))
            .GroupBy(t => t.Category)
            .Select(g => new CategoryBreakdownDto
            {
                Category = g.Key,
                Amount = g.Sum(t => t.Amount),
                // Bug 2 fix — calculate percentage
                Percentage = Math.Round((double)(g.Sum(t => t.Amount) / denom) * 100.0, 2)
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        // Bug 3 and 4 fix — Debit only, map Id and Type
        var topTx = debits
            .OrderByDescending(t => t.Amount)
            .Take(5)
            .Select(t => new FinSenseAPI.DTOs.Upload.TransactionDto
            {
                Id = t.Id,
                Date = t.Date,
                Description = t.Description,
                Amount = t.Amount,
                Type = t.Type,
                Category = t.Category
            })
            .ToList();

        return new SpendingSummaryDto
        {
            TotalIncome = totalIncome,
            TotalRefunds = totalRefunds,
            TotalSpending = totalSpending,
            NetSavings = netSavings,
            ByCategory = byCategory,
            TopTransactions = topTx
        };
    }
}
