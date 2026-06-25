using System.Text;
using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class StatDescriptionCatalogTests
{
    [Fact]
    public void ResolveTemplate_picks_simple_effect_line_when_companion_stat_absent()
    {
        StatDescriptionCatalog catalog = ParseSnippet("""
            description
            2 local_flask_effect_+% local_flask_consume_flask_effect_+%_when_used
            1
            lang "English"
            1|# 0 0 "{0}% increased effect"
            negate
            #|-1 0 0 "{0}% reduced effect"
            1|# !0 "{0}% increased effect. -1% to this value when used"
            negate
            #|-1 !0 "{0}% reduced effect. -1% to this value when used"
            """);

        var modStats = new Dictionary<string, (int Min, int Max)>(StringComparer.OrdinalIgnoreCase)
        {
            ["local_flask_effect_+%"] = (25, 25),
        };

        string? template = catalog.ResolveTemplate(
            "local_flask_effect_+%",
            25,
            25,
            modStats);

        Assert.Equal("#% increased effect", template);
    }

    [Fact]
    public void ResolveTemplate_picks_reduced_duration_when_roll_range_is_negative()
    {
        StatDescriptionCatalog catalog = ParseSnippet("""
            description
            2 local_flask_duration_+% local_flask_consume_flask_duration_+%_when_used
            1
            lang "English"
            1|# 0 0 "{0}% increased Duration"
            negate
            #|-1 0 0 "{0}% reduced Duration"
            1|# !0 "{0}% increased Duration. -1% to this value when used"
            negate
            #|-1 !0 "{0}% reduced Duration. -1% to this value when used"
            """);

        var modStats = new Dictionary<string, (int Min, int Max)>(StringComparer.OrdinalIgnoreCase)
        {
            ["local_flask_duration_+%"] = (-27, -23),
        };

        string? template = catalog.ResolveTemplate(
            "local_flask_duration_+%",
            -27,
            -23,
            modStats);

        Assert.Equal("#% reduced Duration", template);
    }

    [Fact]
    public void ResolveTemplate_picks_reduced_curse_effect_for_negative_only_rolls()
    {
        StatDescriptionCatalog catalog = ParseSnippet("""
            description
            1 local_self_curse_effect_+%_during_flask_effect
            1
            lang "English"
            1|# "{0}% increased Effect of Curses on you during Effect"
            negate
            #|-1 "{0}% reduced Effect of Curses on you during Effect"
            """);

        var modStats = new Dictionary<string, (int Min, int Max)>(StringComparer.OrdinalIgnoreCase)
        {
            ["local_self_curse_effect_+%_during_flask_effect"] = (-65, -60),
        };

        string? template = catalog.ResolveTemplate(
            "local_self_curse_effect_+%_during_flask_effect",
            -65,
            -60,
            modStats);

        Assert.Equal("#% reduced Effect of Curses on you during Effect", template);
    }

    private static StatDescriptionCatalog ParseSnippet(string text)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(text);
        (StatDescriptionCatalog catalog, _) = StatDescriptionParser.ParseEnglishTemplatesFromFiles([("stat_descriptions.txt", bytes)]);
        return catalog;
    }
}
