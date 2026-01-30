namespace CoreOne.Winforms;

/// <summary>
/// Factory interface for creating controls based on property types
/// </summary>
public interface IPropertyControlFactory
{
    /// <summary>
    /// Gets the priority of this factory. Higher priority factories are checked first.
    /// Default priority is 0. Attribute-based factories should use higher priority (e.g., 100).
    /// </summary>
    int Priority => 0;

    /// <summary>
    /// Determines if this factory can handle the given property type
    /// </summary>
    bool CanHandle(Metadata property);

    /// <summary>
    /// Creates an appropriate control for the given property
    /// </summary>
    (Control? control, Action<object?>? setValue) CreateControl(Metadata property, object model, Action<object?> onValueChanged);
}