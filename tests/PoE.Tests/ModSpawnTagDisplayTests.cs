using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModSpawnTagDisplayTests
{
    [Theory]
    [InlineData("utility_flask", "Utility Flask")]
    [InlineData("life_flask", "Life Flask")]
    [InlineData("body_armour", "Body Armour")]
    [InlineData("cluster", "Cluster Jewel")]
    public void GetDisplayName_maps_known_tags(string tag, string expected) =>
        Assert.Equal(expected, ModSpawnTagDisplay.GetDisplayName(tag));

    [Theory]
    [InlineData("Utility Flask", "utility_flask")]
    [InlineData("Life Flask", "life_flask")]
    [InlineData("utility_flask", "utility_flask")]
    public void TryGetCanonicalTag_accepts_display_and_raw_values(string input, string expected)
    {
        Assert.True(ModSpawnTagDisplay.TryGetCanonicalTag(input, out string canonical));
        Assert.Equal(expected, canonical);
    }

    [Fact]
    public void Normalize_resolves_display_names_for_filtering()
    {
        Assert.Equal("utility_flask", ModSpawnTagFilter.Normalize("Utility Flask"));
        Assert.Equal("flask", ModSpawnTagFilter.Normalize("Flask"));
    }
}
