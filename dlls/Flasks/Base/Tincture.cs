using PoE.dlls.Automation;
using PoE.dlls.InteropServices;
using Poss.Win.Automation.Input;

namespace PoE.dlls.Flasks.Base
{
    internal class Tincture : IFlask
    {
        public Flask Flask { get; set; }
        public InputSimulator Input => _inputHost.Simulator;

        private readonly InputSimulatorHost _inputHost;
        private readonly FlaskTiming _timing;
        private bool _isReady;

        public Tincture(InputSimulatorHost inputHost, string key, int number, FlaskTiming timing, FlaskRegistration? saved = null)
        {
            _inputHost = inputHost;
            _timing = timing;

            ResolutionType resolution = InteropHelper.GetScreenResolution();
            var coordinates = GetCoordinates(resolution);

            number--;

            Color top = saved is not null
                ? saved.TopColor
                : InteropHelper.GetColorAt(coordinates.x + coordinates.offset * number, coordinates.y);

            Flask = new Flask(
                FlaskType.Tincture,
                coordinates.x + coordinates.offset * number,
                coordinates.y,
                coordinates.x_bottom + coordinates.offset * number,
                coordinates.y_bottom,
                top,
                FlaskDualPixelReadiness.TinctureCooldownBottomColor,
                key);
        }

        public bool IsReady => _isReady;

        public void UpdateReadiness(ScreenPixelCapture capture) =>
            _isReady = FlaskDualPixelReadiness.TinctureIsReady(
                capture.GetColorAt(Flask.X, Flask.Y),
                capture.GetColorAt(Flask.X_Bottom, Flask.Y_Bottom),
                Flask.Top);

        public async Task Drink(CancellationToken cancellationToken)
        {
            if (Flask.LastUsed + _timing.TinctureCooldown > DateTimeOffset.Now)
                return;

            if (!_isReady)
                return;

            Input.Send(Flask.Key + " Down");
            await Task.Delay(_timing.KeyPressDelay, cancellationToken).ConfigureAwait(false);
            Input.Send(Flask.Key + " Up");
            await Task.Delay(_timing.KeyPressDelay, cancellationToken).ConfigureAwait(false);

            Flask.LastUsed = DateTimeOffset.Now;
        }

        private (int x, int y, int x_bottom, int y_bottom, int offset) GetCoordinates(ResolutionType resolution)
        {
            int x = 0, y = 0, x_bottom = 0, y_bottom = 0, offset = 0;

            switch (resolution)
            {
                case ResolutionType.UHD:
                    x = 0;
                    x_bottom = 0;
                    y = 0;
                    y_bottom = 0;
                    offset = 61;
                    break;
                case ResolutionType.QHD:
                    x = 458;
                    x_bottom = 417;
                    y = 1326;
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
