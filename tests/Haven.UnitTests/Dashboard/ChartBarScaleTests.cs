using Haven.Application.Dashboard;

namespace Haven.UnitTests.Dashboard;

public sealed class ChartBarScaleTests
{
    [Fact]
    public void Heights_follow_input_values_without_equalizing_small_bars()
    {
        var values = new long[] { 2, 8, 3, 15, 1 };
        var heights = values.Select(value => ChartBarScale.GetHeightPercent(value, values.Max())).ToArray();
        Assert.Equal(new[] { 13, 53, 20, 100, 6 }, heights);
        Assert.Equal(100, heights.Max());
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 10, 0)]
    [InlineData(1, 1, 100)]
    [InlineData(1, 100, 1)]
    [InlineData(15, 1, 100)]
    public void Handles_zero_single_and_outlier_values(long value, long maximum, int expected) =>
        Assert.Equal(expected, ChartBarScale.GetHeightPercent(value, maximum));

    [Fact]
    public void Equal_values_have_equal_heights()
    {
        var heights = new long[] { 1, 1, 1, 1, 1 }.Select(value => ChartBarScale.GetHeightPercent(value, 1));
        Assert.All(heights, height => Assert.Equal(100, height));
    }

    [Fact]
    public void Large_outlier_remains_the_tallest_without_flattening_other_values()
    {
        var values = new long[] { 1, 100, 2, 3, 4 };
        var heights = values.Select(value => ChartBarScale.GetHeightPercent(value, values.Max())).ToArray();
        Assert.Equal(new[] { 1, 100, 2, 3, 4 }, heights);
    }
}
