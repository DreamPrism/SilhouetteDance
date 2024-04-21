using System.Text;

namespace SilhouetteDance.Utility;

public static class Network
{
    public static string BuildQuery(this Dictionary<string, string> payload)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in payload) sb.Append($"{key}={value}&");
        return sb.ToString().TrimEnd('&');
    }
    
    public static string BuildQuery(this Dictionary<string, string> payload, string url)
    {
        var sb = new StringBuilder(url).Append('?');
        foreach (var (key, value) in payload) sb.Append($"{key}={value}&");
        return sb.ToString().TrimEnd('&');
    }
    
    public static Dictionary<string, string> ParseQuery(this string query) => 
        query.Split('&').Select(pair => pair.Split('=')).ToDictionary(kv => kv[0], kv => kv[1]);
}