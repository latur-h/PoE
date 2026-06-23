using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoE.dlls.Logger;
using PoE.dlls.Settings.Macros;
using PoE.dlls.Settings.Mods;
using PoE.dlls.Settings.Notes;

namespace PoE.dlls.Settings
{
    public class UserSettings
    {
        public Settings Settings { get; set; }

        private const string appName = "PoE";
        private const string settingsName = "userSettings.json";

        private readonly string _folderPath;
        private readonly string _settingsFilePath;

        public UserSettings()
        {
            _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
            _settingsFilePath = Path.Combine(_folderPath, settingsName);

            Directory.CreateDirectory(_folderPath);

            Settings = new Settings();
        }

        public Settings LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
                return Settings;

            try
            {
                string json = string.Empty;

                using (FileStream fileStream = new FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(fileStream))
                    json = reader.ReadToEnd();

                if (!string.IsNullOrEmpty(json))
                    Settings = JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();

                Settings.FlaskControls ??= new UIFlaskControls();
                Settings.GameData ??= new PoE.dlls.GameData.GameDataSettings();
                Settings.Macros ??= new MacroSettings();
                Settings.Notes ??= new NotesSettings();
                GambleSettingsMigration.EnsureMigrated(Settings.Modifiers);
                MacroSettingsHelper.EnsureInitialized(Settings.Macros);
                NotesSettingsHelper.EnsureInitialized(Settings.Notes);
            }
            catch (Exception ex)
            {
                AppLog.System(LogSeverity.Error, $"Error loading settings: {ex.Message}");

                try
                {
                    File.Move(_settingsFilePath, _settingsFilePath + ".bak", true);
                }
                catch
                {
                    AppLog.System(LogSeverity.Error, $"Error backing up settings file: {ex.Message}");
                }
            }

            return Settings;
        }
        public void SaveSettings()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);

                using FileStream fileStream = new FileStream(_settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                using StreamWriter writer = new StreamWriter(fileStream);
                writer.Write(json);
            }
            catch (Exception ex)
            {
                AppLog.System(LogSeverity.Error, $"Error saving settings: {ex.Message}");
            }
        }
    }
}
