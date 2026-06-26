using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FinSenseAPI.Services.Interfaces;

public interface IClaudeService
{
    Task<string> CallAsync(string systemPrompt, string userMessage, int maxTokens, CancellationToken ct);
    IAsyncEnumerable<string> StreamAsync(string systemPrompt, string userMessage, CancellationToken ct);
}
