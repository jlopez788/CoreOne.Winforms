namespace CoreOne.Winforms.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public abstract class WhenAttribute(string propertyName, object? expectedValue, ComparisonType comparisonType = ComparisonType.EqualTo) : Attribute
{ /// <summary>
  ///
  /// </summary>
    public ComparisonType ComparisonType { get; } = comparisonType;
    /// <summary>
    /// Gets the expected value for the control to be enabled
    /// </summary>
    public object? ExpectedValue { get; } = expectedValue;
    /// <summary>
    ///
    /// </summary>
    public string PropertyName { get; } = propertyName;

    
}