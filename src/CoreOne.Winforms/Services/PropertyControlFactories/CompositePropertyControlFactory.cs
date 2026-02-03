namespace CoreOne.Winforms.Services.PropertyControlFactories;

/// <summary>
/// Composite factory that delegates to specialized factories based on property type and priority
/// </summary>
public class CompositePropertyControlFactory(IEnumerable<IPropertyControlFactory> factories) : IPropertyControlFactory
{
    private readonly IPropertyControlFactory[] _orderedFactories = [.. factories.OrderByDescending(f => f.Priority)];

    public bool CanHandle(Metadata property) => _orderedFactories.Any(f => f.CanHandle(property));

    public (Control control, Action<object?> setValue)? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        // Try each factory in priority order (highest priority first)
        // This allows specialized factories (like Rating with attributes) to take precedence over generic ones
        return _orderedFactories.Where(f => f.CanHandle(property))
                                 .Select(f => f.CreateControl(property, model, onValueChanged))
                                 .FirstOrDefault(p => p?.control != null);
    }
}