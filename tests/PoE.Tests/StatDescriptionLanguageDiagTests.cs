using System.Text;
using PoE.dlls.GameData;
using Xunit;
using Xunit.Abstractions;

namespace PoE.Tests;

public class StatDescriptionLanguageDiagTests
{
    private readonly ITestOutputHelper _output;

    public StatDescriptionLanguageDiagTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Diagnose_english_section_in_stat_descriptions()
    {
        const string gameFolder = @"L:\PoE";
        if (!Directory.Exists(gameFolder))
            return;

        using GameArchiveSession archive = new(gameFolder);
        if (!archive.TryReadGameFile(
                "metadata/statdescriptions/stat_descriptions.txt",
                out byte[] bytes,
                out string? source,
                out _))
            return;

        _output.WriteLine($"source={source} bytes={bytes.Length}");

        string text = Encoding.Unicode.GetString(bytes);
        if (text.StartsWith('\uFEFF'))
            text = text[1..];

        int strength = text.IndexOf("additional_strength", StringComparison.Ordinal);
        _output.WriteLine($"additional_strength index={strength}");
        if (strength >= 0)
            _output.WriteLine(text.Substring(Math.Max(0, strength - 120), Math.Min(500, text.Length - Math.Max(0, strength - 120))));

        int englishCount = CountOccurrences(text, "lang \"English\"");
        int koreanCount = CountOccurrences(text, "lang \"Korean\"");
        _output.WriteLine($"lang English={englishCount} Korean={koreanCount}");

        (StatDescriptionCatalog catalog, _) = StatDescriptionParser.ParseEnglishTemplatesFromFiles([
            ("stat_descriptions.txt", bytes),
        ]);

        var modStats = new Dictionary<string, (int Min, int Max)>(StringComparer.OrdinalIgnoreCase)
        {
            ["additional_strength"] = (10, 20),
        };

        string? template = catalog.ResolveTemplate("additional_strength", 10, 20, modStats);
        _output.WriteLine($"resolved additional_strength={template}");
        Assert.Equal("# to Strength", template);
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
