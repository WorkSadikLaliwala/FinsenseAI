namespace FinSenseAPI.Config;

public class ClaudeOptions
{
    /// <summary>
    /// List of Groq API keys for rotation. When one key hits a rate limit (429),
    /// the service will automatically try the next key in this list.
    /// </summary>
    public List<string> ApiKeys { get; set; } = new();

    /// <summary>
    /// Backwards-compatible single key. If set, it is added as the first key in ApiKeys.
    /// Prefer using ApiKeys for multi-key rotation support.
    /// </summary>
    public string? ApiKey
    {
        get => ApiKeys.Count > 0 ? ApiKeys[0] : null;
        set
        {
            if (!string.IsNullOrWhiteSpace(value) && !ApiKeys.Contains(value))
                ApiKeys.Insert(0, value);
        }
    }

    public string BaseUrl { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
    public int DefaultMaxTokens { get; set; } = 1000;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
}
