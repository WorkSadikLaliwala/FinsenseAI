namespace FinSenseAPI.Config;

public class ClaudeOptions
{
    /// <summary>
    /// List of Groq API keys for rotation. When one key hits a rate limit (429),
    /// the service will automatically try the next key in this list.
    /// </summary>
    public List<string> ApiKeys { get; set; } = new();

    public string BaseUrl { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
    public int DefaultMaxTokens { get; set; } = 1000;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
}

