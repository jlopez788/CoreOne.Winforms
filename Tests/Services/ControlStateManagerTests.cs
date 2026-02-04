using CoreOne.Winforms.Services;
using CoreOne.Winforms.Models;
using System.Windows.Forms;

namespace Tests.Services;

public class ControlStateManagerTests
{
    [Test]
    public void Constructor_WithControl_InitializesState()
    {
        var button = new Button();

        var manager = new ControlStateManager(button);

        Assert.Multiple(() => {
            Assert.That(manager.State, Is.EqualTo(State.Normal));
            Assert.That(manager.IsHovered, Is.False);
            Assert.That(manager.IsFocused, Is.False);
        });
    }

    [Test]
    public void ThemeType_CanBeSet()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);

        manager.ThemeType = ThemeType.Primary;

        Assert.That(manager.ThemeType, Is.EqualTo(ThemeType.Primary));
    }

    [Test]
    public void Bounds_ReturnsInflatedControlBounds()
    {
        var button = new Button 
        { 
            Location = new System.Drawing.Point(10, 10),
            Size = new System.Drawing.Size(100, 30)
        };

        var manager = new ControlStateManager(button);
        var bounds = manager.Bounds;

        Assert.Multiple(() => {
            // Bounds should be inflated by 3 pixels on each side
            Assert.That(bounds.X, Is.EqualTo(7));
            Assert.That(bounds.Y, Is.EqualTo(7));
            Assert.That(bounds.Width, Is.EqualTo(106));
            Assert.That(bounds.Height, Is.EqualTo(36));
        });
    }

    [Test]
    public void IsHovered_WithNormalState_ReturnsFalse()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);

        Assert.That(manager.IsHovered, Is.False);
    }

    [Test]
    public void Constructor_WithInvalidateParent_SetsFlag()
    {
        var button = new Button();

        var manager = new ControlStateManager(button, invalidateParent: true);

        Assert.That(manager, Is.Not.Null);
        Assert.That(manager.State, Is.EqualTo(State.Normal));
    }

    [Test]
    public void TransitionToken_InitiallyCreated()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);

        Assert.That(manager.TransitionToken, Is.Not.Null);
    }

    [Test]
    public void IsChecked_InitiallyFalse()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);

        Assert.That(manager.IsChecked, Is.False);
    }

    [Test]
    public void IsFocused_InitiallyFalse()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);

        Assert.That(manager.IsFocused, Is.False);
    }
}
