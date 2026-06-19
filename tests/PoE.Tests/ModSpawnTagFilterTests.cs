using Microsoft.Data.Sqlite;
using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModSpawnTagFilterTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("  claw  ", "claw")]
    public void Normalize_trims_and_treats_blank_as_any(string? input, string? expected) =>
        Assert.Equal(expected, ModSpawnTagFilter.Normalize(input));

    [Theory]
    [InlineData("cluster", ModItemKind.ClusterJewel)]
    [InlineData("cluster_jewel", ModItemKind.ClusterJewel)]
    [InlineData("flask", ModItemKind.Flask)]
    [InlineData("jewel", ModItemKind.Jewel)]
    [InlineData("abyss_jewel", ModItemKind.AbyssJewel)]
    [InlineData("abyss_jewel_summoner", ModItemKind.AbyssJewel)]
    [InlineData("searing_eye_jewel", ModItemKind.AbyssJewel)]
    public void TryResolveItemKind_recognizes_aliases(string filter, ModItemKind expected)
    {
        Assert.True(ModSpawnTagFilter.TryResolveItemKind(filter, out ModItemKind kind));
        Assert.Equal(expected, kind);
    }

    [Fact]
    public void TryResolveItemKind_returns_false_for_spawn_tags()
    {
        Assert.False(ModSpawnTagFilter.TryResolveItemKind("claw", out _));
        Assert.False(ModSpawnTagFilter.TryResolveItemKind("ring", out _));
    }
}
