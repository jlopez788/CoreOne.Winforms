namespace CoreOne.Winforms;

public interface IGridLayoutManager
{
    /// <summary>
    /// Calculates grid positions for controls with their column spans
    /// </summary>
    List<GridCell> CalculateLayout(IEnumerable<(Control control, GridColumnSpan columnSpan)> items);

    (TableLayoutPanel view, int height) RenderLayout(IEnumerable<GridCell> cells);

    /// <summary>
    /// Renders layout with support for grouping controls into GroupBox containers
    /// </summary>
    (TableLayoutPanel view, int height) RenderLayout(IEnumerable<PropertyGridItem> items);
}