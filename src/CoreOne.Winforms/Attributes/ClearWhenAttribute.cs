namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ClearWhenAttribute(string propertyName, object? expectedValue, ComparisonType comparisonType = ComparisonType.EqualTo) : WhenAttribute(propertyName, expectedValue, comparisonType)
{
}
