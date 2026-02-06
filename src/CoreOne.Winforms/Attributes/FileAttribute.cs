namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FileAttribute(string filter = "All Files (*.*)|*.*", bool multiselect = false) : Attribute
{
    public string Filter { get; } = filter;
    public bool Multiselect { get; } = multiselect;
}