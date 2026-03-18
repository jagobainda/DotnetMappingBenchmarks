namespace DotnetMappingBenchmarks.Helpers;

public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo MadridTimeZone = ResolveMadridTimeZone();

    private static TimeZoneInfo ResolveMadridTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
        }
    }

    public static TimeZoneInfo GetCetTimeZone() => MadridTimeZone;

    public static DateTimeOffset GetCurrentCetTime()
    {
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, MadridTimeZone);
    }
}
