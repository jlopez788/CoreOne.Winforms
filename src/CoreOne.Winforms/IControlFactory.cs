namespace CoreOne.Winforms;

/// <summary>
/// Factory interface for creating WinForms controls based on property metadata and attributes.
/// Factories use a priority-based chain of responsibility pattern to determine which factory
/// handles each property.
/// </summary>
/// <remarks>
/// <para><strong>Priority System:</strong></para>
/// <list type="bullet">
/// <item><description><strong>100+</strong>: Attribute-based factories (e.g., [Rating], [DropdownSource])</description></item>
/// <item><description><strong>0</strong>: Generic type-based factories (e.g., string → TextBox, int → NumericUpDown)</description></item>
/// <item><description><strong>Negative</strong>: Fallback factories</description></item>
/// </list>
/// <para>Higher priority factories are checked first, allowing attribute-based customization to override default type mappings.</para>
/// </remarks>
public interface IControlFactory
{
    /// <summary>
    /// Gets the priority of this factory. Higher priority factories are checked first.
    /// </summary>
    /// <value>
    /// Default is 0 for generic type-based factories.
    /// Attribute-based factories should use 100 or higher.
    /// </value>
    int Priority => 0;

    /// <summary>
    /// Determines whether this factory can create a control for the specified property.
    /// </summary>
    /// <param name="property">The property metadata containing type information and attributes.</param>
    /// <returns>true if this factory can handle the property; otherwise, false.</returns>
    bool CanHandle(Metadata property);

    /// <summary>
    /// Creates a control with two-way data binding for the specified property.
    /// </summary>
    /// <param name="property">The property metadata.</param>
    /// <param name="model">The model instance containing the property.</param>
    /// <param name="onValueChanged">Callback invoked when the control's value changes.</param>
    /// <returns>
    /// A <see cref="EventControlContext"/> containing the created control and binding delegates,
    /// or null if the control cannot be created.
    /// </returns>
    ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged);
}