namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class WatchPropertyAttribute(string propertyName) : WatchPropertiesAttribute(propertyName)
{
    /// <summary>
    /// Gets the name of the property that should be watched
    /// </summary>
    public string PropertyName { get; } = propertyName;
}