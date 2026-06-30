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

using Microsoft.Extensions.Logging;

namespace FinSenseAPI.Hubs;

public class ChatHub : Hub
{
    private readonly IClaudeService _claude;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IChatRepository _chatRepo;
    private readonly ILogger<ChatHub> _logger;
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _activeStreams = new();

    public ChatHub(IClaudeService claude, ITransactionRepository transactionRepo, IChatRepository chatRepo, ILogger<ChatHub> logger)
    {
        _claude = claude;
        _transactionRepo = transactionRepo;
        _chatRepo = chatRepo;
        _logger = logger;
    }

    // Auth optional. Streams AI response back to caller as chunks.
    public async Task SendMessage(Guid sessionId, string message, ChatHistoryItemDto[] chatHistory, int guestMessageCount)
    {
        _logger.LogInformation("[ChatHub] SendMessage called. SessionId: {SessionId}, Message: '{Message}', HistoryCount: {HistoryCount}", 
            sessionId, message, chatHistory?.Length ?? 0);

        // Link a cancellable token for this connection so we can stop streaming if the connection drops
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(Context.ConnectionAborted);
        _activeStreams[Context.ConnectionId] = linkedCts;
        var ct = linkedCts.Token;
        _logger.LogInformation("[ChatHub] >>> SendMessage ENTERED for session {SessionId}", sessionId);
        // fetch transactions for context (best-effort)
        var transactions = Enumerable.Empty<dynamic>();
        try
        {
            _logger.LogInformation("[ChatHub] Fetching transactions for context...");
            _logger.LogInformation("[ChatHub] About to fetch transactions, pool stats"); // or check pool count if exposed
            var fetched = await _transactionRepo.GetBySessionIdAsync(sessionId, ct);
            if (fetched != null)
            {
                transactions = fetched.Cast<dynamic>();
                _logger.LogInformation("[ChatHub] Fetched {Count} transactions.", transactions.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[ChatHub] Error fetching transactions. Continuing with empty context.");
        }

        // build a minimal prompt combining recent transactions and user message
        var txSummary = string.Join("; ", transactions.Take(10).Select(t => $"{t.Date:yyyy-MM-dd}:{t.Description}:{t.Amount}"));
        var historyText = chatHistory is null ? string.Empty : string.Join("\n", chatHistory.Select(h => $"{h.Role}: {h.Content}"));
        var systemPrompt = $"You are FinSense assistant. Transactions: {txSummary}";
        var userPrompt = (historyText + "\nUser: " + message).Trim();

        try
        {
            _logger.LogInformation("[ChatHub] Starting stream request to AI Service...");
            int chunkCount = 0;
            await foreach (var chunk in _claude.StreamAsync(systemPrompt, userPrompt, ct))
            {
                chunkCount++;
                _logger.LogDebug("[ChatHub] Sending chunk {ChunkIndex} (len: {Length}) to caller.", chunkCount, chunk.Length);
                await Clients.Caller.SendAsync("ReceiveChunk", chunk, ct);
            }

            _logger.LogInformation("[ChatHub] AI stream completed. Total chunks sent: {Count}. Completing call...", chunkCount);
            await Clients.Caller.SendAsync("ReceiveComplete", cancellationToken: ct);

            // save message to DB (fire-and-forget)
            var chatMsg = new ChatMessage
            {
                SessionId = sessionId,
                Role = "user",
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("[ChatHub] Saving message to database...");
            try
            {
                await _chatRepo.SaveMessageAsync(chatMsg, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChatHub] Failed to save chat message — continuing anyway");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[ChatHub] Message stream cancelled by client.");
            await Clients.Caller.SendAsync("ReceiveError", "Request cancelled", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ChatHub] Exception occurred while processing SendMessage.");
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
