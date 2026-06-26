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
    {
        _txRepo = txRepo;
        _analytics = analytics;
        _claude = claudeService;
    }

    public async Task<PredictorResponseDto> PredictMonthEndAsync(PredictorRequestDto dto, CancellationToken ct)
    {
        var month = dto.CurrentDate.Month;
        var year = dto.CurrentDate.Year;
        var dayOfMonth = dto.CurrentDate.Day;

        Console.WriteLine($"CurrentDate received: {dto.CurrentDate}");
        var allTxs = (await _txRepo.GetCurrentMonthBySessionAsync(dto.SessionId, month, year, ct)).ToList();

        var txs = allTxs.Where(t => t.Date.Date <= dto.CurrentDate.Date).ToList();
        Console.WriteLine($"Transactions found: {txs.Count}");

        var totalSpentSoFar = txs
            .Where(t => !string.Equals(t.Type, "Credit", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);

        var totalIncomeSoFar = txs
            .Where(t => string.Equals(t.Type, "Credit", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);
        

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var daysRemaining = Math.Max(0, daysInMonth - dayOfMonth);
        //var dailyAverage = dayOfMonth > 0 ? Math.Round(totalSpentSoFar / dayOfMonth, 2) : 0m;
        //var projectedFutureSpend = Math.Round(dailyAverage * daysRemaining, 2);
        var nonEmiSpend = txs
    .Where(t => !string.Equals(t.Type, "Credit", StringComparison.OrdinalIgnoreCase)
             && !string.Equals(t.Category, "EMI", StringComparison.OrdinalIgnoreCase))
    .Sum(t => t.Amount);

        var dailyAverage = dayOfMonth > 0 ? Math.Round(nonEmiSpend / dayOfMonth, 2) : 0m;
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
