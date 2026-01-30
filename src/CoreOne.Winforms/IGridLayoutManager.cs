namespace CoreOne.Winforms;

public interface IGridLayoutManager
{
    /// <summary>
    /// Calculates grid positions for controls with their column spans
    /// </summary>
    IEnumerable<GridCell> CalculateLayout(IEnumerable<(Control Control, GridColumnSpan ColumnSpan)> items);

    /// <summary>
    /// Renders the grid layout into the container
    /// </summary>
    (TableLayoutPanel view, int height) RenderLayout(IEnumerable<GridCell> gridCells);
}