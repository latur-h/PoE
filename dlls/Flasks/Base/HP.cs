using PoE.dlls.Automation;
using PoE.dlls.Flasks;
using PoE.dlls.InteropServices;
using Poss.Win.Automation.Input;

namespace PoE.dlls.Flasks.Base
{
    internal class HP : IFlask
    {
        public Flask Flask { get; set; }
        public InputSimulator Input => _inputHost.Simulator;

        private readonly InputSimulatorHost _inputHost;
        private readonly FlaskTiming _timing;

        public HP(InputSimulatorHost inputHost, string key, int percent, FlaskTiming timing, FlaskRegistration? saved = null)
        {
            _inputHost = inputHost;
            _timing = timing;

            ResolutionType resolution = InteropHelper.GetScreenResolution();
            var (x, y) = GetCoordinates(resolution, percent);

            Color pixel = saved is not null ? saved.TopColor : InteropHelper.GetColorAt(x, y);

            Flask = new Flask(FlaskType.HP, x, y, pixel, key);
        }

        public async Task Drink()
        {
            if (Flask.LastUsed + _timing.HpMpCooldown > DateTimeOffset.Now)
                return;

            if (InteropHelper.GetColorAt(Flask.X, Flask.Y) != Flask.Top)
            {
                Input.Send(Flask.Key + " Down");
                await Task.Delay(_timing.KeyPressDelay);
                Input.Send(Flask.Key + " Up");
                await Task.Delay(_timing.KeyPressDelay);

                Flask.LastUsed = DateTimeOffset.Now;
            }
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
                    x = 150;
                    y = (int)(1400 - ((1400 - 1175) * ((float)percent / 100)));
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
