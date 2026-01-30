namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class WatchPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the property that should be watched
    /// </summary>
    public string PropertyName { get; }

    public WatchPropertyAttribute(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

        PropertyName = propertyName;
    }
}