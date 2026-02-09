using CoreOne.Collections;

namespace CoreOne.Winforms.Services;

public class RefreshManager : IRefreshManager
{
    private readonly DataList<string, Metadata> DependencyMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly DataList<Metadata, IWatchHandler> Registrations = [];

    public void Clear()
    {
        Registrations.Clear();
        DependencyMap.Clear();
    }

    public void NotifyPropertyChanged(object model, string propertyName, object? newValue)
    {
        var kp = Registrations.FirstOrDefault(p => p.Key.Name.Matches(propertyName));
        kp.Value?.Each(p => p.Refresh(model));

        DependencyMap.Get(propertyName)
               ?.Select(p => Registrations.TryGetValue(p, out var context) ? context : null)
               ?.SelectMany(p => p is null ? [] : p)
               ?.Each(p => p.Refresh(model));
    }

    public void RegisterContext(IWatchHandler context, object model)
    {
        Registrations.Add(context.Property, context);

        context.Refresh(model);
        context.Dependencies.Each(p => DependencyMap.Add(p, context.Property));
    }
}