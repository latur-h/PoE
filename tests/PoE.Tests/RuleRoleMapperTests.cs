using PoE.dlls.Gamble;
using Xunit;

namespace PoE.Tests;

public class RuleRoleMapperTests
{
    [Theory]
    [InlineData(GambleType.Alt, -1, "", RuleRole.Exclude)]
    [InlineData(GambleType.Alt, -5, "foo", RuleRole.Exclude)]
    [InlineData(GambleType.Alt, 0.5, "life", RuleRole.Optional)]
    [InlineData(GambleType.Alt, 0.9, "life", RuleRole.Optional)]
    [InlineData(GambleType.Alt, 1, "life", RuleRole.Required)]
    [InlineData(GambleType.Alt, 3, "life", RuleRole.Required)]
    [InlineData(GambleType.Alt, 0, "", RuleRole.None)]
    [InlineData(GambleType.Alt, 0, "life", RuleRole.None)]
    [InlineData(GambleType.Map, 0, "q80r60ps25", RuleRole.Stat)]
    [InlineData(GambleType.Map, 0, "mods:6", RuleRole.Stat)]
    [InlineData(GambleType.Map, 0, "Currency:40;", RuleRole.Stat)]
    [InlineData(GambleType.Map, 1, "Splitting", RuleRole.Include)]
    [InlineData(GambleType.Map, -1, "Reflect", RuleRole.Exclude)]
    [InlineData(GambleType.Map, 0.5, "Splitting", RuleRole.None)]
    public void FromPriority_maps_legacy_values(GambleType type, double priority, string content, RuleRole expected)
    {
        RuleRole role = RuleRoleMapper.FromPriority(type, (decimal)priority, content);
        Assert.Equal(expected, role);
    }

    [Theory]
    [InlineData(GambleType.Alt, RuleRole.Exclude, -1)]
    [InlineData(GambleType.Alt, RuleRole.Optional, 0.5)]
    [InlineData(GambleType.Alt, RuleRole.Required, 1)]
    [InlineData(GambleType.Alt, RuleRole.None, 0)]
    [InlineData(GambleType.Map, RuleRole.Stat, 0)]
    [InlineData(GambleType.Map, RuleRole.Include, 1)]
    public void ToPriority_round_trips_canonical_values(GambleType type, RuleRole role, double expectedPriority)
    {
        decimal priority = RuleRoleMapper.ToPriority(type, role);
        Assert.Equal((decimal)expectedPriority, priority);
    }

    [Fact]
    public void NormalizePriority_collapses_legacy_magnitudes()
    {
        Assert.Equal(-1, RuleRoleMapper.NormalizePriority(-2));
        Assert.Equal(1, RuleRoleMapper.NormalizePriority(99));
        Assert.Equal(0.5m, RuleRoleMapper.NormalizePriority(0.3m));
        Assert.Equal(0, RuleRoleMapper.NormalizePriority(0));
    }
}
