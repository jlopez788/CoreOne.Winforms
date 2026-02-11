namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class VisibleWhenAttribute(bool isVisible) : Attribute
{
    public bool IsVisible { get; } = isVisible;
}