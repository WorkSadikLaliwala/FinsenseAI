using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using FinSenseAPI.Services.Interfaces;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.Models;
using FinSenseAPI.DTOs.Chat;
using System.Collections.Concurrent;

namespace FinSenseAPI.Hubs;

public class ChatHub : Hub
{
    private readonly IClaudeService _claude;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IChatRepository _chatRepo;
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _activeStreams = new();

    public ChatHub(IClaudeService claude, ITransactionRepository transactionRepo, IChatRepository chatRepo)
    {
        _claude = claude;
        _transactionRepo = transactionRepo;
        _chatRepo = chatRepo;
    }

    // Auth optional. Streams AI response back to caller as chunks.
    public async Task SendMessage(Guid sessionId, string message, ChatHistoryItemDto[] chatHistory, int guestMessageCount)
    {
        // Link a cancellable token for this connection so we can stop streaming if the connection drops
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(Context.ConnectionAborted);
        _activeStreams[Context.ConnectionId] = linkedCts;
        var ct = linkedCts.Token;

        // fetch transactions for context (best-effort)
        var transactions = Enumerable.Empty<dynamic>();
        try
        {
            var fetched = await _transactionRepo.GetBySessionIdAsync(sessionId, ct);
            if (fetched != null) transactions = fetched.Cast<dynamic>();
        }
        catch
        {
            // ignore transaction errors and continue with empty context
        }

        // build a minimal prompt combining recent transactions and user message
        var txSummary = string.Join("; ", transactions.Take(10).Select(t => $"{t.Date:yyyy-MM-dd}:{t.Description}:{t.Amount}"));
        var historyText = chatHistory is null ? string.Empty : string.Join("\n", chatHistory.Select(h => $"{h.Role}: {h.Content}"));
        var systemPrompt = $"You are FinSense assistant. Transactions: {txSummary}";
        var userPrompt = (historyText + "\nUser: " + message).Trim();

        try
        {
            await foreach (var chunk in _claude.StreamAsync(systemPrompt, userPrompt, ct))
            {
                await Clients.Caller.SendAsync("ReceiveChunk", chunk, ct);
            }

            await Clients.Caller.SendAsync("ReceiveComplete", cancellationToken: ct);

            // save message to DB (fire-and-forget)
            var chatMsg = new ChatMessage
            {
                SessionId = sessionId,
                Role = "user",
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            _ = _chatRepo.SaveMessageAsync(chatMsg, ct);
        }
        catch (OperationCanceledException)
        {
            await Clients.Caller.SendAsync("ReceiveError", "Request cancelled", ct);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", ex.Message, ct);
        }
        finally
        {
            // remove active stream token for this connection
            _activeStreams.TryRemove(Context.ConnectionId, out var _);
            linkedCts.Dispose();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_activeStreams.TryRemove(Context.ConnectionId, out var cts))
        {
            try { cts.Cancel(); } catch { }
            try { cts.Dispose(); } catch { }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
