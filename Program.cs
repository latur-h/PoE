using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoE.dlls.Flasks;
using PoE.dlls.Settings;
using Poss.Win.Automation.GlobalHotKeys;
using Poss.Win.Automation.Input;

namespace PoE
{
    internal static class Program
    {
        private const string processName = "PathOfExile.exe";
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
                    services.AddSingleton<GlobalHotKeyManager>();
                    services.AddSingleton(x => new InputSimulator(processName));

                    // Flask manager
                    services.AddSingleton<FlaskManager>();

                    // User settings
                    services.AddSingleton<UserSettings>();

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