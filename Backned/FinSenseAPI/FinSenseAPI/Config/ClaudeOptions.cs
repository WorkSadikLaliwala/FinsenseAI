namespace FinSenseAPI.Config;

public class ClaudeOptions
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; }
    public string Model { get; set; } = "claude-sonnet-4-6";
    public int DefaultMaxTokens { get; set; } = 1000;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
}
