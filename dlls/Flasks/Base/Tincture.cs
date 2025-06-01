using InputSimulator;
using PoE.dlls.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Flasks.Base
{
    internal class Tincture : IFlask
    {
        public Flask Flask { get; set; }
        public Simulator Input { get; set; }

        private readonly TimeSpan delay = TimeSpan.FromMilliseconds(5);
        private readonly TimeSpan cooldown = TimeSpan.FromMilliseconds(500);

        public Tincture(Simulator simulator, string key, int number)
        {
            Input = simulator;

            ResolutionType resolution = InteropHelper.GetScreenResolution();
            var coordinates = GetCoordinates(resolution);

            number--;

            Color top = InteropHelper.GetColorAt(coordinates.x + coordinates.offset * number, coordinates.y);
            Color bottom = ColorTranslator.FromHtml("#F9D799");

            Flask = new Flask(FlaskType.Tincture, coordinates.x + coordinates.offset * number, coordinates.y, coordinates.x_bottom + coordinates.offset * number, coordinates.y_bottom, top, bottom, key);
        }

        public async Task Drink()
        {
            if (Flask.LastUsed + cooldown > DateTimeOffset.Now)
                return;

            Color color = InteropHelper.GetColorAt(Flask.X_Bottom, Flask.Y_Bottom);

            if (InteropHelper.GetColorAt(Flask.X, Flask.Y) == Flask.Top && InteropHelper.GetColorAt(Flask.X_Bottom, Flask.Y_Bottom) != Flask.Bottom)
            {
                Input.Send(Flask.Key + " Down");
                await Task.Delay(delay);
                Input.Send(Flask.Key + " Up");
                await Task.Delay(delay);

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
