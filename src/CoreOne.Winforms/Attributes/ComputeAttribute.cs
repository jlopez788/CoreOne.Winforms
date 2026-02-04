namespace CoreOne.Winforms.Attributes;

/// <summary>
/// Specifies that a property's value should be automatically computed by calling a method
/// when dependent properties change.
/// </summary>
/// <param name="methodName">The name of the method to call for computing the value.</param>
/// <remarks>
/// <para>The compute method must:</para>
/// <list type="bullet">
/// <item><description>Be a public instance method on the model class</description></item>
/// <item><description>Return a value compatible with the property type</description></item>
/// <item><description>Accept parameters matching the types of watched properties (in order)</description></item>
/// </list>
/// <para>Use with [WatchProperties] to specify which properties trigger recomputation.</para>
/// </remarks>
/// <example>
/// <code>
/// [Compute(nameof(CalculateTotalScore))]
/// [WatchProperties(nameof(Rating), nameof(IsActive))]
/// public decimal TotalScore { get; set; }
///
/// public decimal CalculateTotalScore(int rating, bool isActive)
/// {
///     return isActive ? rating * 10 : 0;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class ComputeAttribute(string methodName) : Attribute
{
    /// <summary>
    /// Gets the name of the method used to compute the property value.
    /// </summary>
    public string MethodName { get; } = methodName;
}