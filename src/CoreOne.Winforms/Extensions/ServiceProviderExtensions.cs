namespace CoreOne.Winforms.Extensions;

public static class ServiceProviderExtensions
{
    public static TargetCreator GetTargetCreator(this IServiceProvider? services)
    {
        var instance = services?.GetService(typeof(TargetCreator)) as TargetCreator;
        return instance ?? new TargetCreator(services);
    }
}