using PoE.dlls.Updates;
using Xunit;

namespace PoE.Tests;

public class AppVersionTests
{
    [Theory]
    [InlineData("v1.0.14", "1.0.14")]
    [InlineData("1.0.14", "1.0.14")]
    [InlineData("V2.3.4", "2.3.4")]
    public void ParseReleaseTag_parses_semver_tags(string tag, string expected)
    {
        Version? version = AppVersion.ParseReleaseTag(tag);

        Assert.NotNull(version);
        Assert.Equal(expected, version!.ToString(3));
    }

    [Fact]
    public void IsNewerThanCurrent_detects_newer_release()
    {
        Version current = AppVersion.Current;
        var newer = new Version(current.Major, current.Minor, current.Build + 1);

        Assert.True(AppVersion.IsNewerThanCurrent(newer));
        Assert.False(AppVersion.IsNewerThanCurrent(current));
    }
}
