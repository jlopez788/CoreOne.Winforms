namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class IgnoreAttribute : Attribute
{
}