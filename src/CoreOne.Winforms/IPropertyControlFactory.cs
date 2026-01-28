namespace CoreOne.Winforms;

/// <summary>
/// Factory interface for creating controls based on property types
/// </summary>
public interface IPropertyControlFactory
{
    /// <summary>
    /// Determines if this factory can handle the given property type
    /// </summary>
    bool CanHandle(Metadata property);

    /// <summary>
    /// Creates an appropriate control for the given property
    /// </summary>
    (Control? control, Action<object?>? setValue) CreateControl(Metadata property, object model, Action<object?> onValueChanged);
}