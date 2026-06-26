using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FinSenseAPI.Services.Interfaces;
using FinSenseAPI.DTOs.Chat;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.DTOs.Analytics;

namespace FinSenseAPI.Services;

public class ChatService : IChatService
{
    private readonly IClaudeService _claude;
    private readonly ITransactionRepository _txRepo;
    private readonly IAnalyticsService _analytics;
    private readonly IChatRepository _chatRepo;
    private readonly IUploadSessionRepository _sessionRepo;

    public ChatService(IClaudeService claude, ITransactionRepository txRepo, IAnalyticsService analytics, IChatRepository chatRepo, IUploadSessionRepository sessionRepo)
    {
        _claude = claude;
        _txRepo = txRepo;
        _analytics = analytics;
        _chatRepo = chatRepo;
        _sessionRepo = sessionRepo;
    }

    public async Task<ChatResponseDto> SendMessageAsync(ChatRequestDto dto, bool isGuest, CancellationToken ct)
    {
        const int guestLimit = 3;

        // Guest limit check
        if (isGuest && dto.GuestMessageCount >= guestLimit)
        {
            return new ChatResponseDto
            {
                Reply = "You have reached the free message limit. Please register to continue.",
                IsGuest = true,
                MessagesRemaining = 0,
                LimitReached = true
            };
        }
        // Build compact context to limit token usage
        var summary = await _analytics.GetSummaryAsync(dto.SessionId, ct);
        var transactions = (await _txRepo.GetBySessionIdAsync(dto.SessionId, ct)).OrderByDescending(t => t.Date).Take(10).ToList();

        var context = BuildChatContext(summary, transactions);

        var systemContextJson = JsonSerializer.Serialize(new
        {
            income = summary?.TotalIncome ?? 0m,
            spending = summary?.TotalSpending ?? 0m,
            savings = summary?.NetSavings ?? 0m,
            topCategories = summary?.ByCategory?.Take(5).Select(c => new { c.Category, c.Amount }) ?? Enumerable.Empty<object>(),
            recent = transactions.Select(t => new { date = t.Date.ToString("yyyy-MM-dd"), t.Description, t.Amount }).ToList()
        });

        // Use Claude to get reply
        var reply = await _claude.CallAsync(systemContextJson, dto.Message, 800, ct);

        // Save message and return simple response (persistence handled elsewhere in workflow)
        return new ChatResponseDto { Reply = reply, IsGuest = isGuest, MessagesRemaining = 0, LimitReached = false };
    }

    public async Task DeleteHistoryAsync(Guid sessionId, Guid userId, CancellationToken ct)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
        if (session == null) throw new InvalidOperationException("Session not found");
        if (session.UserId == null || session.UserId != userId) throw new UnauthorizedAccessException("No permission to delete history");

        await _chatRepo.DeleteBySessionIdAsync(sessionId, ct);
    }

    private string BuildChatContext(SpendingSummaryDto summary, List<FinSenseAPI.Models.Transaction> recent)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Total Income: ₹{summary?.TotalIncome ?? 0m}");
        sb.AppendLine($"Total Spending: ₹{summary?.TotalSpending ?? 0m}");
        sb.AppendLine("Top Categories:");
        if (summary?.ByCategory != null)
        {
            foreach (var c in summary.ByCategory.Take(5))
            {
                sb.AppendLine($"- {c.Category}: ₹{c.Amount}");
            }
        }

        sb.AppendLine("Recent Transactions:");
        foreach (var t in recent)
        {
            sb.AppendLine($"{t.Date:yyyy-MM-dd} {t.Description} ₹{t.Amount}");
        }

        return sb.ToString();
    }
}
