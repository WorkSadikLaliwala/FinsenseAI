using FinSenseAPI.Config;
using FinSenseAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FinSenseAPI.Services;

public class ClaudeService : IClaudeService
{
    private readonly ClaudeOptions _options;
    private static readonly int[] RetryDelaysMs = { 1000, 2000, 4000 };
    private static readonly HttpClient _httpClient = new HttpClient();

    public ClaudeService(IOptions<ClaudeOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> CallAsync(string systemPrompt, string userMessage, int maxTokens, CancellationToken ct)
    {
        var attempts = Math.Max(1, _options.MaxRetries);
        var url = "https://api.groq.com/openai/v1/chat/completions";

        for (int attempt = 0; attempt <= attempts; attempt++)
        {
            try
            {
                var messages = new List<object>();

                if (!string.IsNullOrWhiteSpace(systemPrompt))
                    messages.Add(new { role = "system", content = systemPrompt });

                var userContent = string.IsNullOrWhiteSpace(userMessage) ? "Process the above." : userMessage;
                messages.Add(new { role = "user", content = userContent });

                var payload = new
                {
                    model = _options.Model,
                    max_tokens = maxTokens,
                    temperature = 0.1,
                    messages
                };

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(payload),
                        Encoding.UTF8,
                        "application/json"
                    )
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

                // Use custom timeout via linked cancellation token source if configured
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

                var response = await _httpClient.SendAsync(request, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var text = await response.Content.ReadAsStringAsync(cts.Token);
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

    public async IAsyncEnumerable<string> StreamAsync(
        string systemPrompt,
        string userMessage,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var url = "https://api.groq.com/openai/v1/chat/completions";
        var messages = new List<object>();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(new { role = "system", content = systemPrompt });
        var userContent = string.IsNullOrWhiteSpace(userMessage) ? "Process the above." : userMessage;
        messages.Add(new { role = "user", content = userContent });
        var payload = new
        {
            model = _options.Model,
            max_tokens = _options.DefaultMaxTokens,
            temperature = 0.1,
            messages,
            stream = true
        };
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            )
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        HttpResponseMessage? response = null;
        string? requestError = null;
        try
        {
            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
        }
        catch (Exception ex)
        {
            requestError = $"Error executing AI request: {ex.Message}";
        }

        if (requestError != null || response == null)
        {
            yield return requestError ?? "Unknown error executing AI request.";
            yield break;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errText = await response.Content.ReadAsStringAsync(cts.Token);
            yield return $"Error calling AI service: {response.StatusCode} - {errText}";
            yield break;
        }
        using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new System.IO.StreamReader(stream);
        while (true)
        {
            cts.Token.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cts.Token);
            if (line == null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6).Trim();
                if (data == "[DONE]") break;
                string? contentChunk = null;
                try
                {
                    using var doc = JsonDocument.Parse(data);
                    var choices = doc.RootElement.GetProperty("choices");
                    if (choices.GetArrayLength() > 0)
                    {
                        var delta = choices[0].GetProperty("delta");
                        if (delta.TryGetProperty("content", out var contentProp))
                        {
                            contentChunk = contentProp.GetString();
                        }
                    }
                }
                catch
                {
                    // Ignore malformed json lines
                }
                if (!string.IsNullOrEmpty(contentChunk))
                {
                    yield return contentChunk;
                }
            }
        }
    }
}
