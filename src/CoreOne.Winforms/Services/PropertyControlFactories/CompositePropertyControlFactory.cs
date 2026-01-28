namespace CoreOne.Winforms.Services.PropertyControlFactories;

/// <summary>
/// Composite factory that delegates to specialized factories based on property type
/// </summary>
public class CompositePropertyControlFactory(IEnumerable<IPropertyControlFactory> factories) : IPropertyControlFactory
{
    public bool CanHandle(Metadata property) => factories.Any(f => f.CanHandle(property));

    public (Control? control, Action<object?>? setValue) CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        // Try each factory that can handle the property type
        // This allows specialized factories (like dropdown) to take precedence
        return factories.Where(f => f.CanHandle(property))
                        .Select(f => f.CreateControl(property, model, onValueChanged))
                        .FirstOrDefault(p => p.control != null);
    }
}