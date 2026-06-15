using PoE.dlls.Automation;
using PoE.dlls.Flasks;
using PoE.dlls.Flasks.Base;
using PoE.dlls.Gamba;
using PoE.dlls.Gamble;
using PoE.dlls.Macros;
using PoE.dlls.Settings;
using PoE.dlls.Settings.Macros;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;
using PoE.dlls.Settings.Mods;

namespace PoE
{
    public partial class Main
    {
        private Gambler? Gambler;

        private async Task Init()
        {
            _hotkeys.Start();

            _hotkeys.Register("Register Flask", RegisterFlasks, _settings.FlaskControls.RegisterKey);
            _hotkeys.Register("Drink Flask", DrinkFlasks, _settings.FlaskControls.DrinkKey);
            _hotkeys.Register("Stop Drinking", StopDrinking, _settings.FlaskControls.StopKey);

            _hotkeys.Register("Gambler get coordinates", GamblerGetCoordinates, _settings.Modifiers.GetCoorinatesKey);
            _hotkeys.Register("Gambler start", GamblerStart, _settings.Modifiers.GamblerStart);
            _hotkeys.Register("Gambler stop", GamblerStop, _settings.Modifiers.GamblerStop);

            MacroSettingsHelper.EnsureInitialized(_settings.Macros);
            MacroHotkeyBinder.RegisterEnableHotkey(_hotkeys, _macroEngine, _settings.Macros.EnableKey);
            RestoreRegisteredFlasks();
            ApplyMacrosRuntime();
            _macroEngine.Start();

            await Task.CompletedTask;
        }

        #region Flasks
        private Task RegisterFlasks()
        {
            Invoke(() =>
            {
                _flaskManager.Flush();
                RegisterFlaskSlot("1", captureColors: true);
                RegisterFlaskSlot("2", captureColors: true);
                RegisterFlaskSlot("3", captureColors: true);
                RegisterFlaskSlot("4", captureColors: true);
                RegisterFlaskSlot("5", captureColors: true);
            });

            return Task.CompletedTask;
        }

        private void RestoreRegisteredFlasks()
        {
            _flaskManager.Flush();

            if (!_settings.Flasks.Values.Any(f => f.IsRegistered))
                return;

            RegisterFlaskSlot("1", captureColors: false);
            RegisterFlaskSlot("2", captureColors: false);
            RegisterFlaskSlot("3", captureColors: false);
            RegisterFlaskSlot("4", captureColors: false);
            RegisterFlaskSlot("5", captureColors: false);
        }

        private void RegisterFlaskSlot(string slotKey, bool captureColors)
        {
            UIFlask ui = _settings.Flasks[slotKey];
            if (!ui.Active)
                return;

            FlaskType type = Enum.Parse<FlaskType>(ui.FlaskType);
            int number = int.Parse(slotKey);

            if (type is FlaskType.HP or FlaskType.MP)
                number = ui.Percent;

            FlaskRegistration? saved = null;

            if (captureColors)
            {
                saved = FlaskRegistrationSampler.Sample(type, number);
                ui.IsRegistered = true;
                ui.RegisteredTopArgb = saved.TopArgb;
                ui.RegisteredBottomArgb = saved.BottomArgb;
            }
            else if (ui.IsRegistered)
            {
                saved = new FlaskRegistration
                {
                    TopArgb = ui.RegisteredTopArgb,
                    BottomArgb = ui.RegisteredBottomArgb,
                };
            }

            _flaskManager.RegisterFlask(type, number, ui.Key, saved);
        }
        private Task DrinkFlasks() => _flaskManager.DrinkFlasks();
        private Task StopDrinking()
        {
            _flaskManager.Stop();
            return Task.CompletedTask;
        }
        #endregion
        #region Gambler
        private Task GamblerGetCoordinates()
        {
            Invoke(() =>
            {
                if (TryApplyMacroCoordinateCapture())
                    return;

                TryApplyRecordedCoordinate();
            });

            return Task.CompletedTask;
        }

        private bool TryApplyMacroCoordinateCapture()
        {
            var coordinates = InteropHelper.GetMousePos();

            if (_macrosPanel.TryApplyCapture(coordinates))
            {
                ClearCoordinateRecording();
                return true;
            }

            return false;
        }

        private void DisarmMacroCoordinateCapture() => _macrosPanel.DisarmCapture();

        private async Task GamblerStart()
        {
            if (Gambler is not null && Gambler.IsRunning())
                return;

            var preset = _settings.Modifiers.GetActivePreset();

            List<Rule> rules = preset.Rules
                .Where(r => !string.IsNullOrEmpty(r.Content))
                .Take(GambleModeLayout.MaxRules)
                .Select(r => new Rule(r.Priority, r.ModifierType, r.Tier, r.Content))
                .ToList();

            if (rules.Count == 0)
                return;

            var coords = GambleCoordinateResolver.Resolve(
                _settings.Modifiers.GambleType,
                _settings.Modifiers.Items,
                _settings.Modifiers.Orbs);

            Gambler = new Gambler(this, _inputHost, TimeSpan.FromMilliseconds(_settings.Modifiers.Delay), _settings.Modifiers.Speed,
                _settings.Modifiers.GambleType, coords.Item, coords.Base, coords.Second, coords.Third, rules);

            await Gambler.StartGambling();

            Gambler = null;
        }
        private Task GamblerStop()
        {
            if (Gambler is not null && Gambler.IsRunning())
                Gambler.StopGambling();

            return Task.CompletedTask;
        }
        #endregion
    }
}
