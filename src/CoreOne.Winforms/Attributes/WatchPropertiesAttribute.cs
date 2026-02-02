namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class WatchPropertiesAttribute(params string[] propertyNames) : Attribute
{
    /// <summary>
    /// Gets the names of the properties that should be watched
    /// </summary>
    public HashSet<string> PropertyNames { get; } = new HashSet<string>(propertyNames ?? [], MStringComparer.OrdinalIgnoreCase);
}