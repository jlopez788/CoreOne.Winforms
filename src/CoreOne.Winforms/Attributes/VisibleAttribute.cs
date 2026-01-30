namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class VisibleAttribute(bool isVisible) : Attribute
{
    public bool IsVisible { get; } = isVisible;
}