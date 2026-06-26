using PoE.dlls.Automation;
using PoE.dlls.Flasks;
using PoE.dlls.InteropServices;
using Poss.Win.Automation.Input;

namespace PoE.dlls.Flasks.Base
{
    internal class MP : IFlask
    {
        public Flask Flask { get; set; }
        public InputSimulator Input => _inputHost.Simulator;

        private readonly InputSimulatorHost _inputHost;
        private readonly FlaskTiming _timing;
        private bool _shouldDrink;

        public MP(InputSimulatorHost inputHost, string key, int percent, FlaskTiming timing, FlaskRegistration? saved = null)
        {
            _inputHost = inputHost;
            _timing = timing;

            ResolutionType resolution = InteropHelper.GetScreenResolution();
            var (x, y) = GetCoordinates(resolution, percent);

            Color pixel = saved is not null ? saved.TopColor : InteropHelper.GetColorAt(x, y);

            Flask = new Flask(FlaskType.MP, x, y, pixel, key);
        }

        public bool IsReady => true;

        public void UpdateReadiness(ScreenPixelCapture capture) =>
            _shouldDrink = capture.GetColorAt(Flask.X, Flask.Y) != Flask.Top;

        public async Task Drink(CancellationToken cancellationToken)
        {
            if (Flask.LastUsed + _timing.HpMpCooldown > DateTimeOffset.Now)
                return;

            if (!_shouldDrink)
                return;

            Input.Send(Flask.Key + " Down");
            await Task.Delay(_timing.KeyPressDelay, cancellationToken).ConfigureAwait(false);
            Input.Send(Flask.Key + " Up");
            await Task.Delay(_timing.KeyPressDelay, cancellationToken).ConfigureAwait(false);

            Flask.LastUsed = DateTimeOffset.Now;
        }

        private (int x, int y) GetCoordinates(ResolutionType resolution, int percent)
        {
            int x = 0, y = 0;

            switch (resolution)
            {
                case ResolutionType.UHD:
                    x = 0;
                    y = 0;
                    break;
                case ResolutionType.QHD:
                    x = 2400;
                    y = (int)(1420 - ((1420 - 1180) * ((float)percent / 100)));
                    break;
                case ResolutionType.FullHD:
                    x = 0;
                    y = 0;
                    break;
                case ResolutionType.HD:
                    x = 0;
                    y = 0;
                    break;
                default:
                    throw new NotSupportedException("Unsupported resolution type.");
            }

            return (x, y);
        }
    }
}
