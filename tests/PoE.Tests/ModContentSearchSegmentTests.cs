using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModContentSearchSegmentTests
{
    [Fact]
    public void Resolve_without_pipe_uses_whole_content()
    {
        var segment = ModContentSearchSegment.Resolve("reflect cannot", caret: 8);

        Assert.Equal(0, segment.Start);
        Assert.Equal("reflect cannot", segment.Phrase);
    }

    [Fact]
    public void Resolve_with_pipes_uses_active_segment()
    {
        var segment = ModContentSearchSegment.Resolve("reflect|cannot regen|twinned", caret: 15);

        Assert.Equal("cannot regen", segment.Phrase);
        Assert.Equal(8, segment.Start);
        Assert.Equal(12, segment.Length);
    }

    [Fact]
    public void Resolve_with_pipes_uses_first_segment_at_start()
    {
        var segment = ModContentSearchSegment.Resolve("reflect|cannot regen", caret: 3);

        Assert.Equal("reflect", segment.Phrase);
    }

    [Fact]
    public void ReplaceSegment_replaces_only_active_pipe_segment()
    {
        string result = ModContentSearchSegment.ReplaceSegment("reflect|blin|twinned", caret: 10, "Hexwarded");

        Assert.Equal("reflect|Hexwarded|twinned", result);
    }

    [Fact]
    public void ReplaceSegment_without_pipe_replaces_whole_content()
    {
        string result = ModContentSearchSegment.ReplaceSegment("blin", caret: 2, "of Blinding");

        Assert.Equal("of Blinding", result);
    }
}
