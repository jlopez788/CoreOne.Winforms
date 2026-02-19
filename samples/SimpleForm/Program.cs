using CoreOne.Winforms;
using CoreOne.Winforms.Extensions;
using Microsoft.Extensions.DependencyInjection;
using SimpleFormExample.Providers;

namespace SimpleFormExample;

public static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Setup dependency injection
        var services = new ServiceCollection();

        // Register CoreOne.Winforms services (ModelBinder, factories, etc.)
        services.AddFormServices();

        // Register dropdown providers
        services.AddSingleton<CountryProvider>();
        services.AddSingleton<StateProvider>();
        services.AddSingleton<IndustryProvider>();

        using var serviceProvider = services.BuildServiceProvider();

        // Create and run main form
        Startup.Run<MainForm>(serviceProvider);
    }
}