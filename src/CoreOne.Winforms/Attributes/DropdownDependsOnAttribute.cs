namespace CoreOne.Winforms.Attributes;

/// <summary>
/// Specifies that a dropdown source depends on another property's value
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class DropdownDependsOnAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the property this dropdown depends on
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Creates a dropdown dependency attribute
    /// </summary>
    /// <param name="propertyName">Name of the property that affects this dropdown's items</param>
    public DropdownDependsOnAttribute(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

        PropertyName = propertyName;
    }
}