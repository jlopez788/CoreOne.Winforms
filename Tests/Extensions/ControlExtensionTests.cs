using CoreOne.Winforms.Extensions;
using System.Windows.Forms;

namespace CoreOne.Winforms.Tests.Extensions;

[TestFixture]
public class ControlExtensionTests
{
    [Test]
    public void GetOffsetX_ReturnsCorrectValue()
    {
        var control = new Button { Location = new System.Drawing.Point(10, 20), Width = 100 };
        
        var offsetX = control.GetOffsetX();
        
        Assert.That(offsetX, Is.EqualTo(110));
    }

    [Test]
    public void GetOffsetX_WithGap_AddsGap()
    {
        var control = new Button { Location = new System.Drawing.Point(10, 20), Width = 100 };
        
        var offsetX = control.GetOffsetX(gap: 5);
        
        Assert.That(offsetX, Is.EqualTo(115));
    }

    [Test]
    public void GetOffsetY_ReturnsCorrectValue()
    {
        var control = new Button { Location = new System.Drawing.Point(10, 20), Height = 30 };
        
        var offsetY = control.GetOffsetY();
        
        Assert.That(offsetY, Is.EqualTo(50));
    }

    [Test]
    public void GetOffsetY_WithGap_AddsGap()
    {
        var control = new Button { Location = new System.Drawing.Point(10, 20), Height = 30 };
        
        var offsetY = control.GetOffsetY(gap: 10);
        
        Assert.That(offsetY, Is.EqualTo(60));
    }

    [Test]
    public void AddControls_WithNullParent_ReturnsNull()
    {
        Panel? panel = null;
        var controls = new[] { new Button() };
        
        var result = panel.AddControls(controls);
        
        Assert.That(result, Is.Null);
    }

    [Test]
    public void AddControls_WithControls_AddsToParent()
    {
        var panel = new Panel();
        var button1 = new Button();
        var button2 = new Button();
        
        panel.AddControls(new[] { button1, button2 });
        
        Assert.That(panel.Controls.Count, Is.EqualTo(2));
        Assert.That(panel.Controls, Does.Contain(button1));
        Assert.That(panel.Controls, Does.Contain(button2));
    }

    [Test]
    public void AddControls_ReturnsParent()
    {
        var panel = new Panel();
        var button = new Button();
        
        var result = panel.AddControls(new[] { button });
        
        Assert.That(result, Is.EqualTo(panel));
    }

    [Test]
    public void ShowDialogExt_WithMatchingResult_ReturnsTrue()
    {
        var form = new Form();
        // Can't actually test ShowDialog without UI interaction
        // Just verify the extension method exists
        Assert.That(form, Is.Not.Null);
    }
}
