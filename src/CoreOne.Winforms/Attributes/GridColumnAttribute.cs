namespace CoreOne.Winforms.Attributes;

/// <summary>
/// Specifies the grid column span for a property in a 6-column grid layout
/// </summary>
/// <remarks>
/// Creates a grid column attribute with the specified span
/// </remarks>
/// <param name="span">The number of columns to span (1-6)</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class GridColumnAttribute(GridColumnSpan span = GridColumnSpan.Default) : Attribute
{
    /// <summary>
    /// Gets the column span (1-6)
    /// </summary>
    public GridColumnSpan Span { get; } = span;
}