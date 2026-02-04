namespace CoreOne.Winforms.Attributes;

/// <summary>
/// Specifies that a property should use a dropdown control with items provided by
/// a registered <see cref="IDropdownSourceProvider"/>.
/// </summary>
/// <param name="type">The type of the dropdown source provider (must implement <see cref="IDropdownSourceProvider"/>).</param>
/// <remarks>
/// <para>The provider type must:</para>
/// <list type="bullet">
/// <item><description>Implement <see cref="IDropdownSourceProviderSync"/> or <see cref="IDropdownSourceProviderAsync"/></description></item>
/// <item><description>Be registered in the dependency injection container</description></item>
/// </list>
/// <para><strong>Cascading Dropdowns:</strong></para>
/// <para>Use [WatchProperties] to create dependent dropdowns that refresh when other properties change.</para>
/// </remarks>
/// <example>
/// <code>
/// // Country dropdown (independent)
/// [DropdownSource(typeof(CountryProvider))]
/// public string Country { get; set; }
///
/// // State dropdown (depends on Country)
/// [DropdownSource(typeof(StateProvider))]
/// [WatchProperties(nameof(Country))]
/// public string State { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DropdownSourceAttribute(Type type) : SourceTypeAttribute(type)
{ }

/// <summary>
/// Generic variant of <see cref="DropdownSourceAttribute"/> for type-safe provider specification.
/// </summary>
/// <typeparam name="T">The dropdown source provider type.</typeparam>
/// <example>
/// <code>
/// [DropdownSource&lt;CountryProvider&gt;]
/// public string Country { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class DropdownSourceAttribute<T> : DropdownSourceAttribute where T : IDropdownSourceProvider
{
    /// <summary>
    /// Initializes a new instance using the specified provider type.
    /// </summary>
    public DropdownSourceAttribute() : base(typeof(T))
    {
    }
}