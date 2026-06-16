using PoE.dlls.Automation;
using PoE.dlls.Gamble;
using Xunit;

namespace PoE.Tests;

public class GambleInputReleaseHelperTests
{
    [Fact]
    public void ReleaseAll_does_not_throw()
    {
        var host = new InputSimulatorHost();
        GambleInputReleaseHelper.ReleaseAll(host);
    }

    [Fact]
    public void ReleaseModifiers_does_not_throw()
    {
        var host = new InputSimulatorHost();
        GambleInputReleaseHelper.ReleaseModifiers(host);
    }
}
