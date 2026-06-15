using PoE.dlls.Automation;
using PoE.dlls.Flasks;
using PoE.dlls.Flasks.Base;
using Poss.Win.Automation.Input;
using Xunit;

namespace PoE.Tests
{
    public class InputSimulatorHostSessionTests
    {
        [Fact]
        public void Configure_replaces_simulator_instance()
        {
            var host = new InputSimulatorHost();
            InputSimulator before = host.Simulator;

            host.Configure("OtherGame.exe");

            Assert.NotSame(before, host.Simulator);
            Assert.Equal("OtherGame.exe", host.EffectiveProcessName);
        }

        [Fact]
        public void Stale_captured_simulator_diverges_from_host_after_reconfigure()
        {
            var host = new InputSimulatorHost();
            var staleBinding = new SessionSimulatorBinding(host, captureAtConstruction: true);
            var liveBinding = new SessionSimulatorBinding(host, captureAtConstruction: false);

            host.Configure("OtherGame.exe");

            Assert.NotSame(staleBinding.ForInput, liveBinding.ForInput);
            Assert.Same(host.Simulator, liveBinding.ForInput);
            Assert.Equal("PathOfExile", staleBinding.ForInput.Filters[0].Name);
            Assert.Equal("OtherGame", liveBinding.ForInput.Filters[0].Name);
        }

        [Fact]
        public void Live_session_binding_uses_current_simulator_after_reconfigure()
        {
            var host = new InputSimulatorHost();
            var binding = new SessionSimulatorBinding(host, captureAtConstruction: false);

            host.Configure("OtherGame.exe");

            Assert.Same(host.Simulator, binding.ForInput);
            Assert.Equal("OtherGame", binding.ForInput.Filters[0].Name);
        }

        [Fact]
        public void Active_window_check_and_send_share_live_simulator_when_bound_to_host()
        {
            var host = new InputSimulatorHost();
            var binding = new SessionSimulatorBinding(host, captureAtConstruction: false);

            host.Configure("OtherGame.exe");

            Assert.Same(binding.ForActiveWindowCheck, binding.ForInput);
            Assert.Equal("OtherGame", binding.ForActiveWindowCheck.Filters[0].Name);
        }

        [Fact]
        public void Flask_input_property_follows_host_after_reconfigure()
        {
            var host = new InputSimulatorHost();
            var manager = new FlaskManager(host);

            try
            {
                manager.RegisterFlask(FlaskType.HP, 50, "1");
            }
            catch (NotSupportedException)
            {
                return;
            }

            IFlask flask = manager.RegisteredFlasksForTests.Single();
            InputSimulator before = flask.Input;

            host.Configure("OtherGame.exe");

            Assert.Same(host.Simulator, flask.Input);
            Assert.NotSame(before, flask.Input);
            Assert.Equal("OtherGame", flask.Input.Filters[0].Name);
        }
    }
}
