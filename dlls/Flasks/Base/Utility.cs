using PoE.dlls.InteropServices;
using Poss.Win.Automation.Input;

namespace PoE.dlls.Flasks.Base
{
    internal class Utility : IFlask
    {
        public Flask Flask { get; set; }
        public InputSimulator Input { get; set; }

        private readonly FlaskTiming _timing;

        public Utility(InputSimulator simulator, string key, int number, FlaskTiming timing)
        {
            Input = simulator;
            _timing = timing;

            ResolutionType resolution = InteropHelper.GetScreenResolution();
            var (x, y, x_bottom, y_bottom, offset) = GetCoordinates(resolution);

            number--;

            Color top = InteropHelper.GetColorAt(x + offset * number, y);
            Color bottom = ColorTranslator.FromHtml("#F9D799");

            Flask = new Flask(FlaskType.Utility, x + offset * number, y, x_bottom + offset * number, y_bottom, top, bottom, key);
        }

        public async Task Drink()
        {
            if (Flask.LastUsed + _timing.UtilityCooldown > DateTimeOffset.Now)
                return;

            if (InteropHelper.GetColorAt(Flask.X, Flask.Y) == Flask.Top && InteropHelper.GetColorAt(Flask.X_Bottom, Flask.Y_Bottom) != Flask.Bottom)
            {
                Input.Send(Flask.Key + " Down");
                await Task.Delay(_timing.KeyPressDelay);
                Input.Send(Flask.Key + " Up");
                await Task.Delay(_timing.KeyPressDelay);

                Flask.LastUsed = DateTimeOffset.Now;
            }
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
