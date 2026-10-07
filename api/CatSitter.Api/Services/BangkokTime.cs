namespace CatSitter.Api.Services;

/// <summary>
/// "Today" for daily checklists and monthly earnings is a Thai calendar day, not a UTC one.
/// Thailand has no DST, so a fixed +07:00 offset is exact. Values written to Postgres
/// (timestamptz) must still be UTC — use <see cref="At"/>, which converts.
/// </summary>
public static class BangkokTime
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(7);

    public static DateOnly Today => DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(Offset).DateTime);

    public static DateTimeOffset At(DateOnly date, int hour, int minute) =>
        new DateTimeOffset(date.ToDateTime(new TimeOnly(hour, minute)), Offset).ToUniversalTime();
}
