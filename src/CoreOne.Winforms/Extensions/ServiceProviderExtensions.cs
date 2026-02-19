using CoreOne.Winforms.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreOne.Winforms.Extensions;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddControlFactoriesFromTypeAssembly<T>(this IServiceCollection services) where T : class => RegisterWatchContextHandlers<T, IControlFactory>(services);

    public static IServiceCollection AddFormServices(this IServiceCollection services)
    {
        services
            .AddControlFactoriesFromTypeAssembly<IControlFactory>()
            .AddWatchHandlersFromTypeAssembly<IWatchFactory>()
            .AddSingleton<FormContainerHostService>()
            .AddSingleton<IPropertyGridItemFactory, PropertyGridItemFactory>()
            .AddSingleton<IGridLayoutManager, GridLayoutManager>()
            .AddSingleton<IModelBinder, ModelBinder>()
            .TryAddScoped<TargetCreator>();

        return services;
    }

    public static IServiceCollection AddWatchHandlersFromTypeAssembly<T>(this IServiceCollection services) where T : class => RegisterWatchContextHandlers<T, IWatchFactory>(services);

    public static TargetCreator GetTargetCreator(this IServiceProvider? services)
    {
        var instance = services?.GetService(typeof(TargetCreator)) as TargetCreator;
        return instance ?? new TargetCreator(services);
    }

    private static IServiceCollection RegisterWatchContextHandlers<TAssembly, TFilter>(IServiceCollection services) where TAssembly : class
    {
        var assembly = typeof(TAssembly).Assembly;
        var handlerType = typeof(TFilter);
        var handlerTypes = assembly.GetTypes()
           .Where(t => t.IsClass && !t.IsAbstract && handlerType.IsAssignableFrom(t))
           .ToList();

        handlerTypes.Each(p => services.AddSingleton(handlerType, p));
        return services;
    }
}