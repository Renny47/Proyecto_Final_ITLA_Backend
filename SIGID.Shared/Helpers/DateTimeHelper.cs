namespace SIGID.Shared.Helpers;

public static class DateTimeHelper
{
    public static DateTime UtcNow => DateTime.UtcNow;
    
    public static DateTime ToUtc(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
    }
    
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
    }
    
    public static long ToUnixTimestamp(DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }
    
    public static string ToIso8601(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }
    
    public static DateTime? ParseIso8601(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString))
            return null;
            
        if (DateTime.TryParse(dateString, out var result))
            return result;
            
        return null;
    }
}