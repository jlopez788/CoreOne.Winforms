namespace CoreOne.Winforms;

/// <summary>
/// Interface for creating property grid items with labels and controls
/// </summary>
public interface IPropertyGridItemFactory
{
    /// <summary>
    /// Creates a property grid item with label above the control
    /// </summary>
    PropertyGridItem? CreatePropertyGridItem(Metadata property, object model, Action<object?> onValueChanged);
}