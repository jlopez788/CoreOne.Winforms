namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SourceTypeAttribute(Type type) : Attribute
{
    /// <summary>
    /// Gets the type of source for the dropdown
    /// </summary>
    public Type SourceType { get; } = type;
}
