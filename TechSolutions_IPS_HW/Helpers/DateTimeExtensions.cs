namespace TechSolutions_IPS_HW.Helpers;

/// <summary>
/// Extension methods for converting UTC <see cref="DateTime"/> values to the local display timezone.
/// The application stores all timestamps in UTC; this helper converts them for display only.
/// </summary>
public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo _localZone =
        TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");

    /// <summary>
    /// Converts a UTC <see cref="DateTime"/> to Local time Time and formats it.
    /// We use this to ensure the times are stored in UTC in the database, but can be displayed 
    /// based on the localisation
    /// </summary>
    public static string ToLocalDisplay(this DateTime utcDate, string format = "dd MMM yyyy HH:mm")
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcDate, DateTimeKind.Utc), _localZone);
        return local.ToString(format);
    }

    /// <summary>
    /// Converts a nullable UTC <see cref="DateTime"/> to Local Time and formats it,
    /// or returns the specified fallback string.
    /// </summary>
    public static string ToLocalDisplay(this DateTime? utcDate, string format = "dd MMM yyyy HH:mm", string fallback = "—")
    {
        return utcDate.HasValue ? utcDate.Value.ToLocalDisplay(format) : fallback;
    }
}
