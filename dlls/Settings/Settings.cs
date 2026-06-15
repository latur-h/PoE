using PoE.dlls.GameData;
using PoE.dlls.Settings.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Settings
{
    public class Settings
    {
        public Dictionary<string, UIFlask> Flasks = [];
        public UIFlaskControls FlaskControls { get; set; } = new();
        public UIModifiers Modifiers { get; set; }
        public GameDataSettings GameData { get; set; } = new();
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }

        public Settings()
        {
            Flasks = new Dictionary<string, UIFlask>
            {
                { "1", new UIFlask { Active = false, Key = "1", FlaskType = "HP", Percent = 50 } },
                { "2", new UIFlask { Active = false, Key = "2", FlaskType = "HP", Percent = 50 } },
                { "3", new UIFlask { Active = false, Key = "3", FlaskType = "HP", Percent = 50 } },
                { "4", new UIFlask { Active = false, Key = "4", FlaskType = "HP", Percent = 50 } },
                { "5", new UIFlask { Active = false, Key = "5", FlaskType = "HP", Percent = 50 } }
            };

            Modifiers = new UIModifiers();
        }
    }
}
