using CoreOne.Winforms.Extensions;
using Microsoft.Extensions.DependencyInjection;
using SimpleFormExample.Models;
using SimpleFormExample.Providers;
using System.Windows.Forms;

namespace SimpleFormExample;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
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

        var serviceProvider = services.BuildServiceProvider();

        // Create and run main form
        var mainForm = new MainForm(serviceProvider);
        Application.Run(mainForm);
    }
}
