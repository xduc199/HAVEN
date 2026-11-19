namespace Haven.Application.Dashboard;

/// <summary>Converts a real count to a proportional bar height without inventing values.</summary>
public static class ChartBarScale
{
    public static int GetHeightPercent(long value, long maximum)
    {
        if (value <= 0 || maximum <= 0) return 0;
        if (value >= maximum) return 100;
        return (int)Math.Max(1, (decimal)value * 100m / maximum);
    }
}
