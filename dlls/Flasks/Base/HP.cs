using InputSimulator;
using PoE.dlls.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Flasks.Base
{
    internal class HP : IFlask
    {
        public Flask Flask { get; set; }
        public Simulator Input { get; set; }

        private readonly TimeSpan delay = TimeSpan.FromMilliseconds(5);
        private readonly TimeSpan cooldown = TimeSpan.FromSeconds(2);

        public HP(Simulator simulator, string key, int percent)
        {
            Input = simulator;

            ResolutionType resolution = InteropHelper.GetScreenResolution();
            var (x, y) = GetCoordinates(resolution, percent);

            Color pixel = InteropHelper.GetColorAt(x, y);

            Flask = new Flask(FlaskType.HP, x, y, pixel, key);
        }

        public async Task Drink()
        {
            if (Flask.LastUsed + cooldown > DateTimeOffset.Now)
                return;

            if (InteropHelper.GetColorAt(Flask.X, Flask.Y) != Flask.Top)
            {
                Input.Send(Flask.Key + " Down");
                await Task.Delay(delay);
                Input.Send(Flask.Key + " Up");
                await Task.Delay(delay);

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
