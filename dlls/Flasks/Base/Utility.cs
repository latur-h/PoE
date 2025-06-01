using InputSimulator;
using PoE.dlls.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Flasks.Base
{
    internal class Utility : IFlask
    {
        public Flask Flask { get; set; }
        public Simulator Input { get; set; }

        private readonly TimeSpan delay = TimeSpan.FromMilliseconds(5);
        private readonly TimeSpan cooldown = TimeSpan.FromMicroseconds(500);

        public Utility(Simulator simulator, string key, int number)
        {
            Input = simulator;

            ResolutionType resolution = InteropHelper.GetScreenResolution();
            var (x, y, x_bottom, y_bottom, offset) = GetCoordinates(resolution);

            number--;

            Color top = InteropHelper.GetColorAt(x + offset * number, y);
            Color bottom = ColorTranslator.FromHtml("#F9D799");

            Flask = new Flask(FlaskType.Utility, x + offset * number, y, x_bottom + offset * number, y_bottom, top, bottom, key);
        }

        public async Task Drink()
        {
            if (Flask.LastUsed + cooldown > DateTimeOffset.Now)
                return;

            if (InteropHelper.GetColorAt(Flask.X, Flask.Y) == Flask.Top && InteropHelper.GetColorAt(Flask.X_Bottom, Flask.Y_Bottom) != Flask.Bottom)
            {
                Input.Send(Flask.Key + " Down");
                await Task.Delay(delay);
                Input.Send(Flask.Key + " Up");
                await Task.Delay(delay);

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
