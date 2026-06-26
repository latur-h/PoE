using PoE.dlls.Automation;
using PoE.dlls.InteropServices;
using Poss.Win.Automation.Input;

namespace PoE.dlls.Flasks.Base
{
    internal class Utility : IFlask
    {
        public Flask Flask { get; set; }
        public InputSimulator Input => _inputHost.Simulator;

        private readonly InputSimulatorHost _inputHost;
        private readonly FlaskTiming _timing;
        private bool _isReady;

        public Utility(InputSimulatorHost inputHost, string key, int number, FlaskTiming timing, FlaskRegistration? saved = null)
        {
            _inputHost = inputHost;
            _timing = timing;

            ResolutionType resolution = InteropHelper.GetScreenResolution();
            var (x, y, x_bottom, y_bottom, offset) = GetCoordinates(resolution);

            number--;

            Color top = saved is not null ? saved.TopColor : InteropHelper.GetColorAt(x + offset * number, y);

            Flask = new Flask(
                FlaskType.Utility,
                x + offset * number,
                y,
                x_bottom + offset * number,
                y_bottom,
                top,
                FlaskDualPixelReadiness.UtilityEffectBottomColor,
                key);
        }

        public bool IsReady => _isReady;

        public void UpdateReadiness(ScreenPixelCapture capture) =>
            _isReady = FlaskDualPixelReadiness.UtilityIsReady(
                capture.GetColorAt(Flask.X, Flask.Y),
                capture.GetColorAt(Flask.X_Bottom, Flask.Y_Bottom),
                Flask.Top);

        public async Task Drink(CancellationToken cancellationToken)
        {
            if (Flask.LastUsed + _timing.UtilityCooldown > DateTimeOffset.Now)
                return;

            if (!_isReady)
                return;

            Input.Send(Flask.Key + " Down");
            await Task.Delay(_timing.KeyPressDelay, cancellationToken).ConfigureAwait(false);
            Input.Send(Flask.Key + " Up");
            await Task.Delay(_timing.KeyPressDelay, cancellationToken).ConfigureAwait(false);

            Flask.LastUsed = DateTimeOffset.Now;
        }

        private static (int x, int y, int x_bottom, int y_bottom, int offset) GetCoordinates(ResolutionType resolution)
        {
            int x = 0, y = 0, x_bottom = 0, y_bottom = 0, offset = 0;

            switch (resolution)
            {
                case ResolutionType.UHD:
                    x = 0;
                    y = 0;
                    x_bottom = 0;
                    y_bottom = 0;
                    offset = 61;
                    break;
                case ResolutionType.QHD:
                    x = 441;
                    x_bottom = 417;
                    y = 1344;
                    y_bottom = 1432;
                    offset = 61;
                    break;
                case ResolutionType.FullHD:
                    x = 0;
                    x_bottom = 0;
                    y = 0;
                    y_bottom = 0;
                    offset = 0;
                    break;
                case ResolutionType.HD:
                    x = 0;
                    x_bottom = 0;
                    y = 0;
                    y_bottom = 0;
                    offset = 0;
                    break;
                default:
                    throw new NotSupportedException("Unsupported resolution type.");
            }

            return (x, y, x_bottom, y_bottom, offset);
        }
    }
}
