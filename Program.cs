using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoE.dlls.Automation;
using PoE.dlls.Flasks;
using PoE.dlls.GameData;
using PoE.dlls.Macros;
using PoE.dlls.Settings;
using Poss.Win.Automation.GlobalHotKeys;

namespace PoE
{
    internal static class Program
    {
        private const string mutexName = "Global\\poe_app";
        private static Mutex? mutex;

        [STAThread]
        static void Main()
        {
            mutex = new(true, mutexName, out bool isNewInstance);
            if (!isNewInstance)
                Environment.Exit(0);

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Automation
                    services.AddSingleton<InputSimulatorHost>();
                    services.AddSingleton<GlobalHotKeyManager>();
                    services.AddSingleton<MacroEngine>();
                    services.AddSingleton<MacroHotkeyBinder>();

                    // Flask manager
                    services.AddSingleton<FlaskManager>();

                    // User settings
                    services.AddSingleton<UserSettings>();

                    // Game data cache
                    services.AddSingleton<ModCacheDatabase>();
                    services.AddSingleton<GameDataRefreshService>();
                    services.AddSingleton<ModSuggestionService>();

                    // Main form
                    services.AddTransient<Main>();
                })
                .Build();

            ApplicationConfiguration.Initialize();

            using var scope = host.Services.CreateScope();
            Main form = scope.ServiceProvider.GetService<Main>()!;

            Application.Run(form);
        }
    }
}