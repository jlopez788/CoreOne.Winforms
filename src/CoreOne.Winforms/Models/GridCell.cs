namespace CoreOne.Winforms.Models;

/// <summary>
/// Represents a cell in the grid layout
/// </summary>
public class GridCell
{
    /// <summary>
    /// Gets or sets the control to display
    /// </summary>
    public Control Control { get; set; } = null!;

    /// <summary>
    /// Gets or sets the column span (1-6)
    /// </summary>
    public GridColumnSpan ColumnSpan { get; set; }

    /// <summary>
    /// Gets or sets the row index
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Gets or sets the column index
    /// </summary>
    public int Column { get; set; }
}