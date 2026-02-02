namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DropdownSourceAttribute(Type type) : SourceTypeAttribute(type)
{ }

/// <summary>
/// Specifies that a string property should use a dropdown control with items from a source type
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class DropdownSourceAttribute<T> : DropdownSourceAttribute where T : IDropdownSourceProvider
{
    public DropdownSourceAttribute() : base(typeof(T))
    {
    }
}