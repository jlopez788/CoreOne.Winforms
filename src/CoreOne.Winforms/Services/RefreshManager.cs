using CoreOne.Collections;

namespace CoreOne.Winforms.Services;

public class RefreshManager : IRefreshManager
{
    private readonly DataList<string, Metadata> DependencyMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Data<Metadata, IWatchHandler> Registrations = [];

    public void Clear()
    {
        Registrations.Clear();
        DependencyMap.Clear();
    }

    public void NotifyPropertyChanged(object model, string propertyName, object? newValue)
    {
        DependencyMap.Get(propertyName)
               ?.Select(p => Registrations.TryGetValue(p, out var context) ? context : null)
               ?.Each(p => p?.Refresh(model));
    }

    public void RegisterContext(IWatchHandler context, object model)
    {
        Registrations.Set(context.Property, context);

        // Build reverse dependency map (property -> contexts that depend on it)
        context.Dependencies.Each(p => DependencyMap.Add(p, context.Property));
        context.Refresh(model);
    }
}