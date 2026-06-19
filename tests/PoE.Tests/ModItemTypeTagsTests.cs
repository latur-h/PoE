using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModItemTypeTagsTests
{
    [Fact]
    public void Claw_includes_shared_weapon_tags_but_not_default()
    {
        IReadOnlyList<string> tags = ModItemTypeTags.GetMatchTags("claw");
        Assert.Contains(tags, t => string.Equals(t, "claw", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(tags, t => string.Equals(t, "weapon", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(tags, t => string.Equals(t, "default", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(tags, t => string.Equals(t, "dagger", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("amulet,weapon", "claw", true)]
    [InlineData("weapon", "claw", true)]
    [InlineData("default", "claw", false)]
    [InlineData("claw", "claw", true)]
    [InlineData("dagger,sceptre,staff,wand", "claw", false)]
    [InlineData("wand", "claw", false)]
    [InlineData("ring", "claw", false)]
    [InlineData("amulet", "ring", false)]
    [InlineData("ring", "ring", true)]
    [InlineData("weapon", "ring", false)]
    [InlineData("boots", "claw", false)]
    [InlineData("boots", "boots", true)]
    [InlineData("bow", "claw", false)]
    [InlineData("claw_shaper", "claw", true)]
    [InlineData("bow,two_hand_weapon,weapon", "claw", false)]
    public void ModMatchesItemType_uses_spawn_tag_intersection(string modTags, string filter, bool expected) =>
        Assert.Equal(expected, ModItemTypeTags.ModMatchesItemType(modTags, filter));
}
