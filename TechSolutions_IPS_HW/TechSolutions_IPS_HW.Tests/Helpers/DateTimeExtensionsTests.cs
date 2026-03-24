using TechSolutions_IPS_HW.Helpers;

namespace TechSolutions_IPS_HW.Tests.Helpers;

public class DateTimeExtensionsTests
{
    [Fact]
    public void ToLocalDisplay_ConvertsUtcToSouthAfricaTime()
    {
        var utc = new DateTime(2026, 01, 15, 10, 00, 00, DateTimeKind.Utc);

        var result = utc.ToLocalDisplay("yyyy-MM-dd HH:mm");

        Assert.Equal("2026-01-15 12:00", result);
    }

    [Fact]
    public void ToLocalDisplay_Nullable_ReturnsFallback_WhenNull()
    {
        DateTime? utc = null;

        var result = utc.ToLocalDisplay(fallback: "N/A");

        Assert.Equal("N/A", result);
    }

    [Fact]
    public void ToLocalDisplay_Nullable_FormatsValue_WhenPresent()
    {
        DateTime? utc = new DateTime(2026, 01, 15, 22, 30, 00, DateTimeKind.Utc);

        var result = utc.ToLocalDisplay("yyyy-MM-dd HH:mm");

        Assert.Equal("2026-01-16 00:30", result);
    }
}
