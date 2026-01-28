using CoreOne.Winforms.Services;
using CoreOne.Winforms.Services.PropertyControlFactories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreOne.Winforms.Extensions;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddFormServices(this IServiceCollection services)
    {
        services.TryAddScoped<TargetCreator>();

        // Register all IPropertyControlFactory implementations as singletons
        RegisterPropertyControlFactories(services);

        services
            .AddSingleton<FormContainerHostService>()
            .AddSingleton<IDropdownRefreshManager, DropdownRefreshManager>()
            .AddSingleton<IPropertyGridItemFactory, PropertyGridItemFactory>()
            .AddSingleton<IGridLayoutManager, GridLayoutManager>()
            .AddSingleton<IModelBinder, ModelBinder>();

        return services;
    }

    public static TargetCreator GetTargetCreator(this IServiceProvider? services)
    {
        var instance = services?.GetService(typeof(TargetCreator)) as TargetCreator;
        return instance ?? new TargetCreator(services);
    }

    private static void RegisterPropertyControlFactories(IServiceCollection services)
    {
        var assembly = typeof(IPropertyControlFactory).Assembly;
        var factoryType = typeof(IPropertyControlFactory);
        var composite = typeof(CompositePropertyControlFactory);
        var factoryTypes = assembly.GetTypes()
           .Where(t => t.IsClass && !t.IsAbstract && factoryType.IsAssignableFrom(t) && t != composite)
           .ToList();

        // Register each factory by its concrete type
        factoryTypes.Each(p => services.AddSingleton(p));

        // Register CompositePropertyControlFactory, resolving concrete types to avoid circular dependency
        services.AddSingleton<IPropertyControlFactory>(sp => {
            var factories = factoryTypes.SelectList(t => (IPropertyControlFactory)sp.GetRequiredService(t));
            return new CompositePropertyControlFactory(factories);
        });
    }
}