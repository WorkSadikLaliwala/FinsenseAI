using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using FinSenseAPI.Services.Interfaces;
using FinSenseAPI.Config;

namespace FinSenseAPI.Services;

public class ClaudeService : IClaudeService
{
    private readonly ClaudeOptions _options;
    private static readonly int[] RetryDelaysMs = { 1000, 2000, 4000 };

    public ClaudeService(IOptions<ClaudeOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> CallAsync(string systemPrompt, string userMessage, int maxTokens, CancellationToken ct)
    {
        var attempts = Math.Max(1, _options.MaxRetries);
        var url = "https://api.groq.com/openai/v1/chat/completions";

        using var http = new HttpClient() { Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds) };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        for (int attempt = 0; attempt <= attempts; attempt++)
        {
            try
            {
                var messages = new List<object>();

                //if (!string.IsNullOrWhiteSpace(systemPrompt))
                //    messages.Add(new { role = "system", content = systemPrompt });

                //messages.Add(new { role = "user", content = string.IsNullOrWhiteSpace(userMessage) ? systemPrompt : userMessage });
                //if (!string.IsNullOrWhiteSpace(systemPrompt))
                //    messages.Add(new { role = "system", content = systemPrompt });

                //// Only add user message if not empty
                //if (!string.IsNullOrWhiteSpace(userMessage))
                //    messages.Add(new { role = "user", content = userMessage });
                if (!string.IsNullOrWhiteSpace(systemPrompt))
                    messages.Add(new { role = "system", content = systemPrompt });

                // Always ensure there's a user message
                var userContent = string.IsNullOrWhiteSpace(userMessage) ? "Process the above." : userMessage;
                messages.Add(new { role = "user", content = userContent });

                var payload = new
                {
                    model = _options.Model,
                    max_tokens = maxTokens,
                    temperature = 0.1,
                    messages
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await http.PostAsync(url, content, ct);

                if (response.IsSuccessStatusCode)
                {
                    var text = await response.Content.ReadAsStringAsync(ct);
                    using var doc = JsonDocument.Parse(text);
                    return doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? string.Empty;
                }

                if ((int)response.StatusCode == 429 || (int)response.StatusCode == 503)
                {
                    if (attempt < attempts)
                    {
                        var delay = RetryDelaysMs[Math.Min(attempt, RetryDelaysMs.Length - 1)];
                        await Task.Delay(delay, ct);
                        continue;
                    }
                    throw new HttpRequestException($"Groq API error: {(int)response.StatusCode} {response.ReasonPhrase}");
                }

                throw new HttpRequestException($"Groq API returned {(int)response.StatusCode}: {response.ReasonPhrase}");
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                if (attempt < attempts)
                {
                    var delay = RetryDelaysMs[Math.Min(attempt, RetryDelaysMs.Length - 1)];
                    await Task.Delay(delay, ct);
                    continue;
                }
                throw;
            }
        }

        throw new HttpRequestException("Groq API call failed after retries.");
    }

    public async IAsyncEnumerable<string> StreamAsync(string systemPrompt, string userMessage, CancellationToken ct)
    {
        // Streaming implementation would use an HTTP client with chunked responses.
        // For now, return a single empty chunk to satisfy the interface.
        yield break;
    }
}
