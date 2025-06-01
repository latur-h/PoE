using GlobalHotKeys;
using InputSimulator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoE.dlls.Flasks;
using PoE.dlls.Settings;

namespace PoE
{
    internal static class Program
    {
        private const string processName = "exe PathOfExile.exe";
        //private const string processName = "Path of Exile";
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
                    // Global hotkey
                    services.AddSingleton<HotKeys>();
                    // Input simulator
                    services.AddSingleton(context => new Simulator(processName));

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