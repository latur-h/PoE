using PoE.dlls.InteropServices;
using Poss.Win.Automation.Input;

namespace PoE.dlls.Flasks.Base
{
    internal class Tincture : IFlask
    {
        public Flask Flask { get; set; }
        public InputSimulator Input { get; set; }

        private readonly FlaskTiming _timing;

        public Tincture(InputSimulator simulator, string key, int number, FlaskTiming timing)
        {
            Input = simulator;
            _timing = timing;

            ResolutionType resolution = InteropHelper.GetScreenResolution();
            var coordinates = GetCoordinates(resolution);

            number--;

            Color top = InteropHelper.GetColorAt(coordinates.x + coordinates.offset * number, coordinates.y);
            Color bottom = ColorTranslator.FromHtml("#F9D799");

            Flask = new Flask(FlaskType.Tincture, coordinates.x + coordinates.offset * number, coordinates.y, coordinates.x_bottom + coordinates.offset * number, coordinates.y_bottom, top, bottom, key);
        }

        public async Task Drink()
        {
            if (Flask.LastUsed + _timing.TinctureCooldown > DateTimeOffset.Now)
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
