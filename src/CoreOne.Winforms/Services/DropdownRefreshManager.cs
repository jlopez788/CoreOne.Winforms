using CoreOne.Collections;

namespace CoreOne.Winforms.Services;

/// <summary>
/// Manages dropdown refresh logic based on property dependencies
/// </summary>
public class DropdownRefreshManager : IDropdownRefreshManager
{
    private readonly DataList<string, Metadata> _dependencyMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Data<Metadata, DropdownContext> _registrations = [];

    public void Clear()
    {
        _registrations.Clear();
        _dependencyMap.Clear();
    }

    public void NotifyPropertyChanged(object model, string propertyName, object? newValue)
    {
        if (!_dependencyMap.TryGetValue(propertyName, out var dependentProperties))
            return;

        // Refresh all dropdowns that depend on this property
        foreach (var dependentProperty in dependentProperties)
        {
            if (_registrations.TryGetValue(dependentProperty, out var context))
            {
                context.RequestRefresh(model);
            }
        }
    }

    public void RegisterDropdown(DropdownContext context, object model)
    {
        _registrations.Set(context.Property, context);

        // Build reverse dependency map (property -> dropdowns that depend on it)
        foreach (var dependencyName in context.Dependencies)
        {
            _dependencyMap.Add(dependencyName, context.Property);
        }

        context.Provider.Initialize(context);
        context.RequestRefresh(model);
    }
}