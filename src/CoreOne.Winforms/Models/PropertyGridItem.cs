using System.Diagnostics;

namespace CoreOne.Winforms.Models;

/// <summary>
/// Represents a property with its associated control and grid configuration
/// </summary>
[DebuggerDisplay("{Display}")]
public class PropertyGridItem(ControlContext controlContext, Metadata property, Action<object?> setValue) : Disposable, IDisposable
{
    /// <summary>
    /// Gets or sets the column span
    /// </summary>
    public GridColumnSpan ColumnSpan { get; set; }
    /// <summary>
    /// Gets or sets the container panel that holds label and input
    /// </summary>
    public Panel Container { get; set; } = null!;
    public ControlContext ControlContext { get; } = controlContext;
    /// <summary>
    /// Gets or sets the error provider for validation display
    /// </summary>
    public ErrorProvider? ErrorProvider { get; set; }
    /// <summary>
    /// Gets or sets the input control
    /// </summary>
    public Control InputControl { get; } = controlContext.Control;
    /// <summary>
    /// Gets or sets the label control
    /// </summary>
    public Label Label { get; set; } = null!;
    /// <summary>
    /// Gets or sets the property metadata
    /// </summary>
    public Metadata Property { get; set; } = property;

    private string Display => $"[{(int)ColumnSpan}] {Property.Name} :: {InputControl.GetType().Name}";

    public void SetValue(object? value) => setValue?.Invoke(value);

    protected override void OnDispose()
    {
        Utility.Try(() => {
            ControlContext.Dispose();
            Label.CrossThread(() => {
                Label.Dispose();
                InputControl.Dispose();
                Container.Dispose();
            });
        });
    }
}