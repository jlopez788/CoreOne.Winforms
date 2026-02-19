namespace CoreOne.Winforms.Services;

public class GridLayoutManager : IGridLayoutManager
{
    private const int MaxColumns = 6;
    private const int RowHeight = 45;

    public List<GridCell> CalculateLayout(IEnumerable<(Control control, GridColumnSpan columnSpan)>? items)
    {
        if (items is null || !items.Any())
            return [];

        var gridCells = new List<GridCell>();
        int currentRow = 0;
        int currentColumn = 0;
        foreach (var item in items.Where(p => p.columnSpan != GridColumnSpan.None))
        {
            var columnSpan = item.columnSpan;
            var control = item.control;
            var validatedSpan = Math.Clamp((int)columnSpan, 1, MaxColumns);

            // Check if we need to move to next row
            if (currentColumn + validatedSpan > MaxColumns)
            {
                currentRow++;
                currentColumn = 0;
            }

            // Create grid cell
            gridCells.Add(new GridCell {
                Control = control,
                ColumnSpan = (GridColumnSpan)validatedSpan,
                Row = currentRow,
                Column = currentColumn
            });

            // Update current column position
            currentColumn += validatedSpan;

            // If we've filled the row, move to next row
            if (currentColumn >= MaxColumns)
            {
                currentRow++;
                currentColumn = 0;
            }
        }

        return gridCells;
    }

    public (TableLayoutPanel view, int height) RenderLayout(IEnumerable<GridCell> cells)
    {
        var cellsList = cells.ToList();        
        if (cellsList.Count == 0)
        {
            return (CreateLayoutPanel(0), 0);
        }
        
        var maxRow = cellsList.Max(r => r.Row);
        var layoutPanel = CreateLayoutPanel(maxRow);
        layoutPanel.SuspendLayout();
        foreach (var cell in cellsList)
        {
            layoutPanel.Controls.Add(cell.Control, cell.Column, cell.Row);
            layoutPanel.SetColumnSpan(cell.Control, (int)cell.ColumnSpan);
        }

        layoutPanel.Height = (maxRow + 1) * RowHeight;

        layoutPanel.ResumeLayout();
        return (layoutPanel, (maxRow + 1) * RowHeight);
    }

    /// <summary>
    /// Renders layout with support for grouping controls into GroupBox containers
    /// </summary>
    public (TableLayoutPanel view, int height) RenderLayout(IEnumerable<PropertyGridItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var rows = CalculateLayout(items.Select(p => ((Control)p.Container, p.ColumnSpan)));
        return rows.Count == 0 ? (CreateLayoutPanel(0), 0) : RenderLayout(rows);
    }

    private static TableLayoutPanel CreateLayoutPanel(int rowCount)
    {
        var layoutPanel = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoScroll = true,
            ColumnCount = MaxColumns,
            RowCount = rowCount,
            Padding = new Padding(10)
        };

        // Create 6 equal columns
        for (int i = 0; i < MaxColumns; i++)
        {
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / MaxColumns));
        }

        // Auto-size rows
        for (int i = 0; i < rowCount; i++)
        {
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        return layoutPanel;
    }
}