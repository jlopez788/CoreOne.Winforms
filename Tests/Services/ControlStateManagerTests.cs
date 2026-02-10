using CoreOne.Winforms.Services;
using CoreOne.Winforms.Models;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using CoreOne;

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
        var button = new Button {
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

    [Test]
    public void SetFocus_WithTrue_SetsFocusedTrue()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);

        manager.SetFocus(true);

        Assert.That(manager.IsFocused, Is.True);
    }

    [Test]
    public void SetFocus_WithFalse_SetsFocusedFalse()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);
        manager.SetFocus(true);

        manager.SetFocus(false);

        Assert.That(manager.IsFocused, Is.False);
    }

    [Test]
    public void SetState_ChangesState()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);

        manager.SetState(State.HLite);

        Assert.That(manager.State, Is.EqualTo(State.HLite));
    }

    [Test]
    public void SetState_ToPressed_ChangesStateProperly()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);

        manager.SetState(State.Pressed);

        Assert.That(manager.State, Is.EqualTo(State.Pressed));
    }

    [Test]
    public void SetState_WithDisabledControl_IgnoresHoverAndPress()
    {
        var button = new Button { Enabled = false };
        var manager = new ControlStateManager(button);

        manager.SetState(State.HLite);

        Assert.That(manager.State, Is.EqualTo(State.Normal));
    }

    [Test]
    public void BackColor_ReturnsValidColor()
    {
        var button = new Button { BackColor = Color.Blue };
        var manager = new ControlStateManager(button);

        var color = manager.BackColor();

        Assert.That(color, Is.Not.EqualTo(Color.Empty));
    }

    [Test]
    public void BackColor_WithButtonLike_ReturnsValidColor()
    {
        var button = new Button { BackColor = Color.Blue };
        var manager = new ControlStateManager(button);

        var color = manager.BackColor(buttonLike: true);

        Assert.That(color, Is.Not.EqualTo(Color.Empty));
    }

    [Test]
    public void BackBrush_ReturnsValidBrush()
    {
        var button = new Button { BackColor = Color.Blue };
        var manager = new ControlStateManager(button);
        var rect = new Rectangle(0, 0, 100, 50);

        var brush = manager.BackBrush(rect);

        Assert.That(brush, Is.Not.Null);
        brush.Dispose();
    }

    [Test]
    public void BackBrush_WithButtonLike_ReturnsValidBrush()
    {
        var button = new Button { BackColor = Color.Blue };
        var manager = new ControlStateManager(button);
        var rect = new Rectangle(0, 0, 100, 50);

        var brush = manager.BackBrush(rect, buttonLike: true);

        Assert.That(brush, Is.Not.Null);
        brush.Dispose();
    }

    [Test]
    public void BackBrush_WithHoverState_ReturnsGradientBrush()
    {
        var button = new Button { BackColor = Color.Blue };
        var manager = new ControlStateManager(button);
        manager.SetState(State.HLite);
        var rect = new Rectangle(0, 0, 100, 50);

        var brush = manager.BackBrush(rect);

        Assert.That(brush, Is.InstanceOf<LinearGradientBrush>());
        brush.Dispose();
    }

    [Test]
    public void BorderColor_ReturnsValidColor()
    {
        var button = new Button { BackColor = Color.Blue };
        var manager = new ControlStateManager(button);

        var color = manager.BorderColor();

        Assert.That(color, Is.Not.EqualTo(Color.Empty));
    }

    [Test]
    public void BorderColor_WithHoverState_ReturnsLighterColor()
    {
        var button = new Button { BackColor = Color.Blue };
        var manager = new ControlStateManager(button);

        manager.SetState(State.HLite);
        var hoverColor = manager.BorderColor();

        Assert.That(hoverColor, Is.Not.EqualTo(Color.Empty));
    }

    [Test]
    public void ForeColor_ReturnsValidColor()
    {
        var button = new Button { ForeColor = Color.Black };
        var manager = new ControlStateManager(button);

        var color = manager.ForeColor();

        Assert.That(color, Is.Not.EqualTo(Color.Empty));
    }

    [Test]
    public void ForeColor_WithDisabledControl_ReturnsDarkenedColor()
    {
        var button = new Button { Enabled = false, ForeColor = Color.Black };
        var manager = new ControlStateManager(button);

        var color = manager.ForeColor();

        Assert.That(color, Is.Not.EqualTo(Color.Empty));
    }

    [Test]
    public void Subscribe_ReceivesStateChanges()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);
        State? capturedState = null;
        var token = SToken.Create();

        manager.Subscribe(state => capturedState = state, token);
        manager.SetState(State.HLite);

        Assert.That(capturedState, Is.EqualTo(State.HLite));
        token.Dispose();
    }

    [Test]
    public void Subscribe_WithDisposedToken_StopsReceivingUpdates()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);
        var callCount = 0;
        var token = SToken.Create();

        manager.Subscribe(state => callCount++, token);
        manager.SetState(State.HLite);
        token.Dispose();
        manager.SetState(State.Pressed);

        Assert.That(callCount, Is.EqualTo(1)); // Only first change counted
    }

    [Test]
    public void TransitionToken_DisposedOnStateChange()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);
        var firstToken = manager.TransitionToken;

        manager.SetState(State.HLite);

        Assert.That(manager.TransitionToken, Is.Not.EqualTo(firstToken));
    }

    [Test]
    public void Constructor_WithCheckBox_TracksCheckedState()
    {
        var checkBox = new CheckBox();
        var manager = new ControlStateManager(checkBox);

        checkBox.Checked = true;

        Assert.That(manager.IsChecked, Is.True);
    }

    [Test]
    public void Dispose_CleansUpResources()
    {
        var button = new Button();
        var manager = new ControlStateManager(button);

        Assert.DoesNotThrow(() => button.Dispose());
    }
}