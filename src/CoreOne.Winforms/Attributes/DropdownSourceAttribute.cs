namespace CoreOne.Winforms.Attributes;

public class DropdownSourceAttribute(Type type) : Attribute
{   /// <summary>
    /// Gets the type that provides the dropdown items
    /// </summary>
    public Type SourceType { get; } = type;
}

/// <summary>
/// Specifies that a string property should use a dropdown control with items from a source type
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class DropdownSourceAttribute<T> : DropdownSourceAttribute where T : IDropdownSourceProviderSync
{
    public DropdownSourceAttribute() : base(typeof(T))
    {
    }
}