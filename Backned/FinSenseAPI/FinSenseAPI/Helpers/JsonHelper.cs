using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FinSenseAPI.Helpers;

public static class JsonHelper
{
    public static string ExtractJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw ?? string.Empty;

        // Strip markdown code fences
        try
        {
            raw = Regex.Replace(raw, "```json|```", "", RegexOptions.IgnoreCase).Trim();
        }
        catch
        {
            // ignore regex errors and proceed
        }

        // Find first { or [ and last } or ]
        int start = raw.IndexOfAny(new[] { '{', '[' });
        int end = raw.LastIndexOfAny(new[] { '}', ']' });
        if (start >= 0 && end > start)
        {
            return raw.Substring(start, end - start + 1);
        }

        return raw;
    }

    public static bool TryDeserialize<T>(string raw, out T? value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var json = ExtractJson(raw);
        try
        {
            value = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return value != null;
        }
        catch (Exception)
        {
            // Log upstream; swallow here and return false
            return false;
        }
    }
}
