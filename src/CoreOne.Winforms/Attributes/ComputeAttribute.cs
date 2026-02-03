namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class ComputeAttribute(string methodName) : Attribute
{
    public string MethodName { get; } = methodName;
}