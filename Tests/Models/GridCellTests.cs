using CoreOne.Winforms.Models;

namespace CoreOne.Winforms.Tests.Models;

[TestFixture]
public class GridCellTests
{
    [Test]
    public void Constructor_DefaultValues()
    {
        var cell = new GridCell();
        Assert.Multiple(() => {
            Assert.That(cell.Control, Is.Null);
            Assert.That(cell.Column, Is.EqualTo(0));
            Assert.That(cell.Row, Is.EqualTo(0));
        });
    }

    [Test]
    public void ColumnSpan_CanBeSet()
    {
        var cell = new GridCell { ColumnSpan = GridColumnSpan.Half };
        
        Assert.That(cell.ColumnSpan, Is.EqualTo(GridColumnSpan.Half));
    }

    [Test]
    public void Control_CanBeSet()
    {
        var control = new System.Windows.Forms.TextBox();
        var cell = new GridCell { Control = control };
        
        Assert.That(cell.Control, Is.EqualTo(control));
    }
}
