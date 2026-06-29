using FinSenseAPI.DTOs.Predictor;
using FinSenseAPI.Prompts;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.Services.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FinSenseAPI.Services;

public class PredictorService : IPredictorService
{
    private readonly ITransactionRepository _txRepo;
    private readonly IAnalyticsService _analytics;
    private readonly IClaudeService _claude;


    public PredictorService(ITransactionRepository txRepo, IAnalyticsService analytics, IClaudeService claudeService)
    {        _txRepo = txRepo;

        _analytics = analytics;
        _claude = claudeService;
    }

    public async Task<PredictorResponseDto> PredictMonthEndAsync(PredictorRequestDto dto, CancellationToken ct)
    {
        var allSessionTxs = (await _txRepo.GetBySessionIdAsync(dto.SessionId, ct)).ToList();
        var referenceDate = dto.CurrentDate;

        // Fallback: If no transactions match the current system month/year, use the latest transaction's date
        if (allSessionTxs.Any() && !allSessionTxs.Any(t => t.Date.Month == referenceDate.Month && t.Date.Year == referenceDate.Year))
        {
            var latestTxDate = allSessionTxs.Max(t => t.Date);
            // Simulate mid-month (e.g. 25th of the month) to show active future spend projections
            var targetDay = Math.Min(latestTxDate.Day, 25);
            referenceDate = new DateTime(latestTxDate.Year, latestTxDate.Month, targetDay);
        }

        var month = referenceDate.Month;
        var year = referenceDate.Year;
        var dayOfMonth = referenceDate.Day;

        Console.WriteLine($"CurrentDate received: {dto.CurrentDate}, ReferenceDate used: {referenceDate}");
        
        var allTxs = allSessionTxs.Where(t => t.Date.Month == month && t.Date.Year == year).ToList();
        var txs = allTxs.Where(t => t.Date.Date <= referenceDate.Date).ToList();
        Console.WriteLine($"Transactions found: {txs.Count}");

        var totalSpentSoFar = txs
            .Where(t => !string.Equals(t.Type, "Credit", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);

        var totalIncomeSoFar = txs
            .Where(t => string.Equals(t.Type, "Credit", StringComparison.OrdinalIgnoreCase)
                     && !(t.Description != null && t.Description.Contains("refund", StringComparison.OrdinalIgnoreCase)))
            .Sum(t => t.Amount);
        

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var daysRemaining = Math.Max(0, daysInMonth - dayOfMonth);

        var dailyAverage = dayOfMonth > 0 ? Math.Round(totalSpentSoFar / dayOfMonth, 2) : 0m;
        var projectedFutureSpend = Math.Round(dailyAverage * daysRemaining, 2);

        var projectedBalance = Math.Round(totalIncomeSoFar - (totalSpentSoFar + projectedFutureSpend), 2);

        var summary = await _analytics.GetSummaryAsync(dto.SessionId, ct);
        var topCategory = summary?.ByCategory?.FirstOrDefault()?.Category ?? string.Empty;

        var status = projectedBalance switch
        {
            < 0 => "Danger",
            < 5000 => "Warning",
            _ => "Safe"
        };
        var topAmount = summary?.ByCategory?.FirstOrDefault()?.Amount ?? 0m;

        var commentary = await _claude.CallAsync(
            ClaudePrompts.MonthEndPredictorCommentary(
                dayOfMonth, totalSpentSoFar, dailyAverage,
                daysRemaining, projectedBalance, totalIncomeSoFar,
                status, topCategory, topAmount),
            "",
            300,
            ct
        );

        return new PredictorResponseDto
        {
            ProjectedBalance = projectedBalance,
            TotalSpentSoFar = totalSpentSoFar,
            ProjectedFutureSpend = projectedFutureSpend,
            DailyAverage = dailyAverage,
            DaysRemaining = daysRemaining,
            MonthlyIncome = totalIncomeSoFar,
            Status = status,
            AICommentary = commentary ?? string.Empty,
            TopSpendingCategory = topCategory
        };
    }
}
