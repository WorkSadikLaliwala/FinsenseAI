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
    private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

    public ClaudeService(IOptions<ClaudeOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Returns the list of API keys to try. Falls back to a single empty string
    /// if no keys are configured (will fail gracefully with an auth error).
    /// </summary>
    private IReadOnlyList<string> GetKeys()
    {
        var keys = _options.ApiKeys?.FindAll(k => !string.IsNullOrWhiteSpace(k));
        if (keys == null || keys.Count == 0)
            return new[] { string.Empty };
        return keys;
    }

    /// <summary>
    /// Builds the JSON payload shared by both CallAsync and StreamAsync.
    /// </summary>
    private static StringContent BuildPayload(object payload)
    {
        return new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
    }

    // ─────────────────────────────────────────────
    // CallAsync  (non-streaming, with key rotation)
    // ─────────────────────────────────────────────
    public async Task<string> CallAsync(
        string systemPrompt,
        string userMessage,
        int maxTokens,
        CancellationToken ct)
    {
        var keys = GetKeys();
        Exception? lastException = null;

        for (int keyIndex = 0; keyIndex < keys.Count; keyIndex++)
        {
            var apiKey = keys[keyIndex];
            Console.WriteLine($"[ClaudeService] CallAsync: trying Key[{keyIndex}]");

            var attempts = Math.Max(1, _options.MaxRetries);

            for (int attempt = 0; attempt <= attempts; attempt++)
            {
                try
                {
                    var messages = new List<object>();
                    if (!string.IsNullOrWhiteSpace(systemPrompt))
                        messages.Add(new { role = "system", content = systemPrompt });

                    var userContent = string.IsNullOrWhiteSpace(userMessage)
                        ? "Process the above."
                        : userMessage;
                    messages.Add(new { role = "user", content = userContent });

                    var payload = new
                    {
                        model = _options.Model,
                        max_tokens = maxTokens,
                        temperature = 0.1,
                        messages
                    };

                    var request = new HttpRequestMessage(HttpMethod.Post, GroqUrl)
                    {
                        Content = BuildPayload(payload)
                    };
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiKey);

                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

                    var response = await _httpClient.SendAsync(request, cts.Token);

                    // ✅ Success
                    if (response.IsSuccessStatusCode)
                    {
                        var text = await response.Content.ReadAsStringAsync(cts.Token);
                        using var doc = JsonDocument.Parse(text);
                        Console.WriteLine($"[ClaudeService] CallAsync: Key[{keyIndex}] succeeded.");
                        return doc.RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString() ?? string.Empty;
                    }

                    var statusCode = (int)response.StatusCode;

                    // 🔄 Rate limited or service unavailable → rotate to next key
                    if (statusCode == 429 || statusCode == 503)
                    {
                        var errBody = await response.Content.ReadAsStringAsync(cts.Token);
                        Console.WriteLine(
                            $"[ClaudeService] Key[{keyIndex}] returned {statusCode} " +
                            $"(attempt {attempt}/{attempts}): {errBody}");

                        // If we have more keys, break inner retry loop and try next key
                        if (keyIndex < keys.Count - 1)
                        {
                            lastException = new HttpRequestException(
                                $"Key[{keyIndex}] rate limited ({statusCode}). Rotating to next key.");
                            break; // break inner attempt loop → outer key loop increments keyIndex
                        }

                        // Last key — still retry with delays
                        if (attempt < attempts)
                        {
                            var delay = RetryDelaysMs[Math.Min(attempt, RetryDelaysMs.Length - 1)];
                            await Task.Delay(delay, ct);
                            continue;
                        }

                        throw new HttpRequestException(
                            $"All {keys.Count} API key(s) exhausted. Last error: {statusCode} {response.ReasonPhrase}");
                    }

                    // ❌ Non-retryable error (400, 401, 404 etc.) — throw immediately
                    throw new HttpRequestException(
                        $"Groq API returned {statusCode}: {response.ReasonPhrase}");
                }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested)
                {
                    Console.WriteLine($"[ClaudeService] Key[{keyIndex}] timed out (attempt {attempt}).");
                    if (attempt < attempts)
                    {
                        var delay = RetryDelaysMs[Math.Min(attempt, RetryDelaysMs.Length - 1)];
                        await Task.Delay(delay, ct);
                        continue;
                    }
                    throw;
                }
            }
        }

        throw lastException ?? new HttpRequestException("Groq API call failed after trying all keys.");
    }

    // ─────────────────────────────────────────────
    // StreamAsync  (SSE streaming, with key rotation)
    // ─────────────────────────────────────────────
    public async IAsyncEnumerable<string> StreamAsync(
        string systemPrompt,
        string userMessage,
        [EnumeratorCancellation] CancellationToken ct)
    {
        Console.WriteLine($"[ClaudeService] StreamAsync started at {DateTime.UtcNow:O}");

        var keys = GetKeys();
        bool success = false;
        string? errorMessage = null;

        for (int keyIndex = 0; keyIndex < keys.Count && !success; keyIndex++)
        {
            var apiKey = keys[keyIndex];
            Console.WriteLine($"[ClaudeService] StreamAsync: trying Key[{keyIndex}]");

            var messages = new List<object>();
            if (!string.IsNullOrWhiteSpace(systemPrompt))
                messages.Add(new { role = "system", content = systemPrompt });

            var userContent = string.IsNullOrWhiteSpace(userMessage)
                ? "Process the above."
                : userMessage;
            messages.Add(new { role = "user", content = userContent });

            var payload = new
            {
                model = _options.Model,
                max_tokens = _options.DefaultMaxTokens,
                temperature = 0.1,
                messages,
                stream = true
            };

            var request = new HttpRequestMessage(HttpMethod.Post, GroqUrl)
            {
                Content = BuildPayload(payload)
            };
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            Console.WriteLine($"[ClaudeService] Timeout set to {_options.TimeoutSeconds}s");

            HttpResponseMessage? response = null;
            string? requestError = null;

            try
            {
                response = await _httpClient.SendAsync(
                    request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                Console.WriteLine(
                    $"[ClaudeService] Key[{keyIndex}] response: {response.StatusCode} at {DateTime.UtcNow:O}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[ClaudeService] Key[{keyIndex}] EXCEPTION before headers: {ex.GetType().Name}: {ex.Message}");
                requestError = $"Error executing AI request: {ex.Message}";
            }

            if (requestError != null || response == null)
            {
                errorMessage = requestError ?? "Unknown error executing AI request.";
                continue; // try next key
            }

            var statusCode = (int)response.StatusCode;

            // 🔄 Rate limited → rotate to next key
            if (statusCode == 429 || statusCode == 503)
            {
                var errText = await response.Content.ReadAsStringAsync(cts.Token);
                Console.WriteLine(
                    $"[ClaudeService] StreamAsync: Key[{keyIndex}] rate limited ({statusCode}). " +
                    $"Rotating to next key. Body: {errText}");
                errorMessage = $"Key[{keyIndex}] rate limited. Trying next key...";
                continue; // try next key
            }

            // ❌ Other non-success error
            if (!response.IsSuccessStatusCode)
            {
                var errText = await response.Content.ReadAsStringAsync(cts.Token);
                errorMessage = $"Error calling AI service: {response.StatusCode} - {errText}";
                // Non-retryable (e.g. 400 bad request) — stop trying other keys
                break;
            }

            // ✅ Success — stream the response
            success = true;
            Console.WriteLine($"[ClaudeService] StreamAsync: Key[{keyIndex}] streaming...");

            using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
            using var reader = new System.IO.StreamReader(stream);

            while (true)
            {
                cts.Token.ThrowIfCancellationRequested();
                var line = await reader.ReadLineAsync(cts.Token);
                if (line == null)
                {
                    Console.WriteLine("[ClaudeService] Stream ended (line null)");
                    break;
                }
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!line.StartsWith("data: ")) continue;

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
                            contentChunk = contentProp.GetString();
                    }
                }
                catch
                {
                    // Ignore malformed SSE lines
                }

                if (!string.IsNullOrEmpty(contentChunk))
                    yield return contentChunk;
            }
        }

        // If no key succeeded, yield the final error message
        if (!success && errorMessage != null)
        {
            Console.WriteLine($"[ClaudeService] StreamAsync: All keys failed. Last error: {errorMessage}");
            yield return $"Error: All API keys exhausted. {errorMessage}";
        }
    }
}
