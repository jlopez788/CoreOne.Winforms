namespace CoreOne.Winforms.Services;

public class GridLayoutManager : IGridLayoutManager
{
    private const int MaxColumns = 6;

    public IEnumerable<GridCell> CalculateLayout(IEnumerable<(Control Control, GridColumnSpan ColumnSpan)> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var gridCells = new List<GridCell>();
        int currentRow = 0;
        int currentColumn = 0;

        foreach (var (control, columnSpan) in items.Where(p => p.ColumnSpan != GridColumnSpan.None))
        {
            // Validate column span
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

    public (TableLayoutPanel view, int height) RenderLayout(IEnumerable<GridCell> gridCells)
    {
        ArgumentNullException.ThrowIfNull(gridCells);

        var cells = gridCells.ToList();
        if (cells.Count == 0)
            return (CreateEmptyLayout(), 45);

        var maxRow = cells.Max(c => c.Row);
        var layoutPanel = CreateLayoutPanel(maxRow + 1);
        foreach (var cell in cells)
        {
            layoutPanel.Controls.Add(cell.Control, cell.Column, cell.Row);
            layoutPanel.SetColumnSpan(cell.Control, (int)cell.ColumnSpan);
        }

        layoutPanel.Height = (maxRow + 1) * 45;

        return (layoutPanel, (maxRow + 1) * 45);
    }

    private static TableLayoutPanel CreateEmptyLayout()
    {
        return new TableLayoutPanel {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(16)
        };
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