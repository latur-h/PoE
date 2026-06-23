using PoE.dlls.UI.Markdown;
using Xunit;

namespace PoE.Tests;

public class MarkdownCopyChipPreprocessorTests
{
    [Fact]
    public void ExpandCopyChips_ReplacesInlineSyntaxWithHtmlChip()
    {
        const string input = "fd [[copy] there is a text to copy] sd";
        string result = MarkdownCopyChipPreprocessor.ExpandCopyChips(input);

        Assert.Contains("class=\"copy-chip\"", result);
        Assert.Contains("data-copy=\"there is a text to copy\"", result);
        Assert.Contains("fd ", result);
        Assert.Contains(" sd", result);
        Assert.DoesNotContain("[[copy]", result);
    }

    [Fact]
    public void ExpandCopyChips_EncodesHtmlInCopyText()
    {
        const string input = "[[copy] a < b & c > d]]";
        string result = MarkdownCopyChipPreprocessor.ExpandCopyChips(input);

        Assert.Contains("data-copy=\"a &lt; b &amp; c &gt; d\"", result);
    }

    [Fact]
    public void ExpandCopyChips_LeavesPlainTextUntouched()
    {
        const string input = "plain text without copy syntax";
        string result = MarkdownCopyChipPreprocessor.ExpandCopyChips(input);

        Assert.Equal(input, result);
    }
}
