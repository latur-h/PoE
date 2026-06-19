using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModCatalogTagHelperTests
{
    [Theory]
    [InlineData("flask", true)]
    [InlineData("affliction_minion_life", true)]
    [InlineData("expansion_jewel_large", true)]
    [InlineData("abyss_jewel_summoner", true)]
    [InlineData("searing_eye_jewel", true)]
    [InlineData("claw", true)]
    [InlineData("unique", false)]
    [InlineData("not_a_tag", false)]
    public void IsAllowedSpawnTag_recognizes_flask_cluster_and_item_tags(string tag, bool expected) =>
        Assert.Equal(expected, ModCatalogTagHelper.IsAllowedSpawnTag(tag));

    [Theory]
    [InlineData("utility_flask", true)]
    [InlineData("life_flask", true)]
    [InlineData("affliction_flask_duration", false)]
    [InlineData("expedition_flask", false)]
    public void IsFlaskSpawnTag_recognizes_equipment_flask_tags(string tag, bool expected) =>
        Assert.Equal(expected, ModCatalogTagHelper.IsFlaskSpawnTag(tag));

    [Fact]
    public void IsFlaskDomain_matches_flask_domain_only() =>
        Assert.True(ModCatalogTagHelper.IsFlaskDomain(ModCatalogTagHelper.DomainFlask));

    [Fact]
    public void ResolveItemKind_prefers_eldritch_over_map()
    {
        ModItemKind kind = ModCatalogTagHelper.ResolveItemKind(
            11,
            ["map"],
            isMap: true,
            ModEldritchInfluence.SearingExarch);

        Assert.Equal(ModItemKind.Eldritch, kind);
    }

    [Fact]
    public void ResolveItemKind_detects_cluster_from_domain()
    {
        ModItemKind kind = ModCatalogTagHelper.ResolveItemKind(
            ModCatalogTagHelper.DomainClusterJewel,
            ["affliction_minion_life"],
            isMap: false,
            ModEldritchInfluence.None);

        Assert.Equal(ModItemKind.ClusterJewel, kind);
    }

    [Fact]
    public void ResolveItemKind_detects_flask_from_flask_domain()
    {
        ModItemKind kind = ModCatalogTagHelper.ResolveItemKind(
            ModCatalogTagHelper.DomainFlask,
            ["utility_flask"],
            isMap: false,
            ModEldritchInfluence.None);

        Assert.Equal(ModItemKind.Flask, kind);
    }

    [Fact]
    public void ResolveItemKind_detects_flask_from_spawn_tag()
    {
        ModItemKind kind = ModCatalogTagHelper.ResolveItemKind(
            1,
            ["flask", "default"],
            isMap: false,
            ModEldritchInfluence.None);

        Assert.Equal(ModItemKind.Flask, kind);
    }

    [Fact]
    public void ResolveItemKind_detects_abyss_from_domain()
    {
        ModItemKind kind = ModCatalogTagHelper.ResolveItemKind(
            ModCatalogTagHelper.DomainAbyssJewel,
            ["abyss_jewel_summoner"],
            isMap: false,
            ModEldritchInfluence.None);

        Assert.Equal(ModItemKind.AbyssJewel, kind);
    }

    [Fact]
    public void EnrichSpawnTags_adds_searing_tag_for_fire_attack_mods()
    {
        IReadOnlyList<string> tags = AbyssJewelSubtypeTags.EnrichSpawnTags(
            "AbyssAddedFireDamageWithDaggersJewel1",
            ["dagger", "abyss_jewel_melee"]);

        Assert.Contains(tags, tag => string.Equals(tag, AbyssJewelSubtypeTags.SearingEye, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void EnrichSpawnTags_does_not_tag_minion_fire_as_searing()
    {
        IReadOnlyList<string> tags = AbyssJewelSubtypeTags.EnrichSpawnTags(
            "AbyssMinionAddedFireDamageJewel1",
            ["abyss_jewel_summoner"]);

        Assert.DoesNotContain(tags, tag => string.Equals(tag, AbyssJewelSubtypeTags.SearingEye, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ResolveItemKind_detects_jewel_from_domain_10()
    {
        ModItemKind kind = ModCatalogTagHelper.ResolveItemKind(
            ModCatalogTagHelper.DomainJewel,
            ["default"],
            isMap: false,
            ModEldritchInfluence.None);

        Assert.Equal(ModItemKind.Jewel, kind);
    }

    [Fact]
    public void ResolveItemKind_detects_jewel_from_spawn_tag()
    {
        ModItemKind kind = ModCatalogTagHelper.ResolveItemKind(
            1,
            ["jewel"],
            isMap: false,
            ModEldritchInfluence.None);

        Assert.Equal(ModItemKind.Jewel, kind);
    }

    [Fact]
    public void FormatSpawnTags_sorts_and_deduplicates()
    {
        string formatted = ModCatalogTagHelper.FormatSpawnTags(["flask", "default", "flask", "ring"]);
        Assert.Equal("default,flask,ring", formatted);
    }

    [Fact]
    public void MergeItemKind_keeps_higher_priority_kind()
    {
        ModItemKind merged = ModCatalogTagHelper.MergeItemKind(ModItemKind.Item, ModItemKind.ClusterJewel);
        Assert.Equal(ModItemKind.ClusterJewel, merged);
    }
}
