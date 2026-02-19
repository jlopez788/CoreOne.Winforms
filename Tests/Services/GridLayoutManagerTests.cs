using CoreOne.Reflection;
using CoreOne.Winforms;
using CoreOne.Winforms.Models;
using CoreOne.Winforms.Services;

namespace Tests.Services;

public class GridLayoutManagerTests
{
    private GridLayoutManager _layoutManager = null!;

    [SetUp]
    public void Setup()
    {
        _layoutManager = new GridLayoutManager();
    }

    [Test]
    public void CalculateLayout_WithEmptyList_ReturnsEmpty()
    {
        var items = Array.Empty<(Control, GridColumnSpan)>();

        var result = _layoutManager.CalculateLayout(items);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void CalculateLayout_WithSingleItem_ReturnsOneCell()
    {
        var control = new Label();
        var items = new (Control, GridColumnSpan)[] { (control, GridColumnSpan.Full) };

        var result = _layoutManager.CalculateLayout(items).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() => {
            Assert.That(result[0].Control, Is.EqualTo(control));
            Assert.That(result[0].Row, Is.EqualTo(0));
            Assert.That(result[0].Column, Is.EqualTo(0));
        });
    }

    [Test]
    public void CalculateLayout_WithMultipleItems_PositionsInGrid()
    {
        var control1 = new Label();
        var control2 = new TextBox();
        var items = new (Control, GridColumnSpan)[]
        {
            (control1, GridColumnSpan.Half),
            (control2, GridColumnSpan.Half)
        };

        var result = _layoutManager.CalculateLayout(items).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.Multiple(() => {
            Assert.That(result[0].Row, Is.EqualTo(0));
            Assert.That(result[0].Column, Is.EqualTo(0));
            Assert.That(result[1].Row, Is.EqualTo(0));
            Assert.That(result[1].Column, Is.EqualTo(3));
        });
    }

    [Test]
    public void CalculateLayout_WhenRowFull_MovesToNextRow()
    {
        var control1 = new Label();
        var control2 = new TextBox();
        var items = new (Control, GridColumnSpan)[]
        {
            (control1, GridColumnSpan.Full),
            (control2, GridColumnSpan.Full)
        };

        var result = _layoutManager.CalculateLayout(items).ToList();

        Assert.Multiple(() => {
            Assert.That(result[0].Row, Is.EqualTo(0));
            Assert.That(result[1].Row, Is.EqualTo(1));
        });
    }

    [Test]
    public void CalculateLayout_WithColumnSpanOverflow_WrapsToNextRow()
    {
        var control1 = new Label();
        var control2 = new TextBox();
        var items = new (Control, GridColumnSpan)[]
        {
            (control1, GridColumnSpan.Four),
            (control2, GridColumnSpan.Half)
        };

        var result = _layoutManager.CalculateLayout(items).ToList();

        Assert.Multiple(() => {
            // control2 should wrap to next row since 4 + 3 > 6
            Assert.That(result[0].Row, Is.EqualTo(0));
            Assert.That(result[1].Row, Is.EqualTo(1));
        });
    }

    [Test]
    public void CalculateLayout_WithNoneSpan_SkipsItem()
    {
        var control1 = new Label();
        var control2 = new TextBox();
        var items = new (Control, GridColumnSpan)[]
        {
            (control1, GridColumnSpan.None),
            (control2, GridColumnSpan.Full)
        };

        var result = _layoutManager.CalculateLayout(items).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Control, Is.EqualTo(control2));
    }

    [Test]
    public void RenderLayout_WithEmptyCells_ReturnsEmptyPanel()
    {
        var cells = Array.Empty<GridCell>();

        var (panel, height) = _layoutManager.RenderLayout(cells);

        Assert.That(panel, Is.Not.Null);
        Assert.Multiple(() => {
            Assert.That(panel.Dock, Is.EqualTo(DockStyle.Fill));
            Assert.That(height, Is.EqualTo(0));
        });
    }

    [Test]
    public void RenderLayout_WithCells_CreatesLayoutPanel()
    {
        var control = new Label();
        var cells = new[]
        {
            new GridCell { Control = control, Column = 0, Row = 0, ColumnSpan = GridColumnSpan.Full }
        };

        var (panel, height) = _layoutManager.RenderLayout(cells);

        Assert.That(panel, Is.Not.Null);
        Assert.Multiple(() => {
            Assert.That(panel.Controls.Contains(control), Is.True);
            Assert.That(height, Is.EqualTo(45));
        });
    }

    [Test]
    public void RenderLayout_WithMultipleRows_CalculatesHeight()
    {
        var control1 = new Label();
        var control2 = new TextBox();
        var cells = new[]
        {
            new GridCell { Control = control1, Column = 0, Row = 0, ColumnSpan = GridColumnSpan.Full },
            new GridCell { Control = control2, Column = 0, Row = 1, ColumnSpan = GridColumnSpan.Full }
        };

        var (panel, height) = _layoutManager.RenderLayout(cells);

        Assert.Multiple(() => {
            Assert.That(panel.Controls.Count, Is.EqualTo(2));
            Assert.That(height, Is.EqualTo(90)); // 2 rows * 45
        });
    }

    [Test]
    public void RenderLayout_SetsColumnSpan()
    {
        var control = new Label();
        var cells = new[]
        {
            new GridCell { Control = control, Column = 0, Row = 0, ColumnSpan = GridColumnSpan.Half }
        };

        var (panel, _) = _layoutManager.RenderLayout(cells);

        var columnSpan = panel.GetColumnSpan(control);
        Assert.That(columnSpan, Is.EqualTo(3));
    }

    #region RenderLayout Tests

    [Test]
    public void RenderLayout_WithEmptyList_ReturnsEmptyPanel()
    {
        var items = Array.Empty<PropertyGridItem>();

        var (control, height) = _layoutManager.RenderLayout(items);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(control, Is.Not.Null);
            Assert.That(control, Is.TypeOf<TableLayoutPanel>());
            Assert.That(height, Is.EqualTo(0));
        }
    }

    [Test]
    public void RenderLayout_WithNoGroups_UsesSimpleLayout()
    {
        var items = new[]
        {
            CreatePropertyGridItem("Field1", GridColumnSpan.Full, null),
            CreatePropertyGridItem("Field2", GridColumnSpan.Full, null)
        };

        var (control, height) = _layoutManager.RenderLayout(items);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(control, Is.TypeOf<TableLayoutPanel>());
            Assert.That(height, Is.EqualTo(90)); // 2 rows * 45
        }
    }

    private PropertyGridItem CreatePropertyGridItem(string name, GridColumnSpan columnSpan, string? groupName)
    {
        var control = new TextBox { Name = name };
        var container = new Panel();
        container.Controls.Add(control);

        var property = MetaType.GetMetadata(typeof(TestModel), nameof(TestModel.TestProperty));
        var controlContext = new TestControlContext(control);

        var item = new PropertyGridItem(controlContext, property, _ => { }) {
            Container = container,
            ColumnSpan = columnSpan
        };

        return item;
    }

    private class TestControlContext(Control control) : ControlContext(control, _ => { })
    {
    }

    private class TestModel
    {
        public string TestProperty { get; set; } = string.Empty;
    }

    #endregion
}