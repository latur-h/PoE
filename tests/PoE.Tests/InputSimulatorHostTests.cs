using PoE.dlls.Automation;
using Xunit;

namespace PoE.Tests
{
    public class InputSimulatorHostTests
    {
        [Theory]
        [InlineData(null, "PathOfExile.exe")]
        [InlineData("", "PathOfExile.exe")]
        [InlineData("   ", "PathOfExile.exe")]
        [InlineData("PathOfExile", "PathOfExile.exe")]
        [InlineData("PathOfExile.exe", "PathOfExile.exe")]
        [InlineData("pathofexile.EXE", "pathofexile.exe")]
        [InlineData("exe PathOfExile", "PathOfExile.exe")]
        public void ToInputSimulatorArgument_always_uses_exe_suffix(string? input, string expected)
        {
            Assert.Equal(expected, InputSimulatorHost.ToInputSimulatorArgument(input));
        }

        [Fact]
        public void Configure_recreates_simulator_with_exe_process_filter()
        {
            var host = new InputSimulatorHost();

            host.Configure("PathOfExile");

            Assert.Equal("PathOfExile.exe", host.EffectiveProcessName);
            Assert.Single(host.Simulator.Filters);
            Assert.Equal(Poss.Win.Automation.Input.WindowFilterKind.Process, host.Simulator.Filters[0].Type);
            Assert.Equal("PathOfExile", host.Simulator.Filters[0].Name);
        }
    }
}
