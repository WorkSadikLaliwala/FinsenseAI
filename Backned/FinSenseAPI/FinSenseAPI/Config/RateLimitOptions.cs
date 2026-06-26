namespace FinSenseAPI.Config;

public class RateLimitOptions
{
    public int MaxRequestsPerMinute { get; set; } = 20;
    public int MaxClaudeCallsPerMinute { get; set; } = 5;
}
