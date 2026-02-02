using CoreOne.Attributes;

namespace CoreOne.Winforms.Attributes;

/// <summary>
/// Specifies that a property's control should be enabled only when a specified condition is met
/// </summary>
/// <param name="propertyName">The name of the property to monitor</param>
/// <param name="expectedValue">The value that enables this control</param>
/// <param name="comparisonType">The type of comparison to perform</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class EnabledWhenAttribute(string propertyName, object? expectedValue, ComparisonType comparisonType = ComparisonType.EqualTo) : WatchPropertiesAttribute(propertyName)
{
    public ComparisonType ComparisonType { get; } = comparisonType;
    /// <summary>
    /// Gets the expected value for the control to be enabled
    /// </summary>
    public object? ExpectedValue { get; } = expectedValue;
    public string PropertyName { get; } = propertyName;
}