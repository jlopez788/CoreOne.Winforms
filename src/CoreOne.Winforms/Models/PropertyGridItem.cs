namespace CoreOne.Winforms.Models;

/// <summary>
/// Represents a property with its associated control and grid configuration
/// </summary>
public class PropertyGridItem(Control input, Metadata property, Action<object?> setValue)
{
    /// <summary>
    /// Gets or sets the property metadata
    /// </summary>
    public Metadata Property { get; set; } = property;

    /// <summary>
    /// Gets or sets the label control
    /// </summary>
    public Label Label { get; set; } = null!;

    /// <summary>
    /// Gets or sets the input control
    /// </summary>
    public Control InputControl { get; set; } = input;

    /// <summary>
    /// Gets or sets the column span
    /// </summary>
    public GridColumnSpan ColumnSpan { get; set; }

    /// <summary>
    /// Gets or sets the container panel that holds label and input
    /// </summary>
    public Panel Container { get; set; } = null!;

    public Action<object?> SetValue { get; set; } = setValue;
}