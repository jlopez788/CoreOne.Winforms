namespace CoreOne.Winforms.Attributes;

public sealed class VisibleAttribute(bool isVisible) : Attribute
{
    public bool IsVisible { get; } = isVisible;
}
