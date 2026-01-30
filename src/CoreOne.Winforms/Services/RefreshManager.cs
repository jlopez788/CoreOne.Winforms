using CoreOne.Collections;

namespace CoreOne.Winforms.Services;

public class RefreshManager : IRefreshManager
{
    private readonly DataList<string, Metadata> DependencyMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Data<Metadata, WatchContext> Registrations = [];

    public void Clear()
    {
        Registrations.Clear();
        DependencyMap.Clear();
    }

    public void NotifyPropertyChanged(object model, string propertyName, object? newValue)
    {
        if (!DependencyMap.TryGetValue(propertyName, out var dependentProperties))
            return;

        // Refresh all contexts that depend on this property
        foreach (var dependentProperty in dependentProperties)
        {
            if (Registrations.TryGetValue(dependentProperty, out var context))
            {
                context.RequestRefresh(model);
            }
        }
    }

    public void RegisterContext(WatchContext context, object model)
    {
        Registrations.Set(context.Property, context);
        // Build reverse dependency map (property -> contexts that depend on it)
        foreach (var dependencyName in context.Dependencies)
        {
            DependencyMap.Add(dependencyName, context.Property);
        }

        context.RequestRefresh(model);
    }
}