using CoreOne.Reactive;
using CoreOne.Winforms.Models;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace CoreOne.Winforms.Services;

public class ControlStateManager
{
    private readonly Control? Control;
    private readonly Func<Rectangle> GetBoundsFn;
    private readonly bool InvalidateParent;
    private readonly Func<bool> IsEnabledFn;
    private readonly BehaviorSubject<State> SubjectState;
    private readonly SToken Token = SToken.Create();
    private readonly ToolStripItem? ToolStripItem;
    private ThemeType PTheme;
    public Rectangle Bounds => GetBoundsFn.Invoke();
    public bool IsChecked { get; private set; }
    public bool IsFocused { get; private set; }
    public bool IsHovered => State == State.HLite;
    public State State => SubjectState.Value;
    public ThemeType ThemeType {
        get => PTheme;
        set {
            PTheme = value;
            SetState(State);
        }
    }
    public SToken TransitionToken { get; private set; } = SToken.Create();
    protected Metadata Checked { get; private set; }
    private Color CBackColor => Control?.BackColor ?? ToolStripItem?.BackColor ?? throw new NullReferenceException("Invalid Control color");
    private Color CForeColor => Control?.ForeColor ?? ToolStripItem?.ForeColor ?? throw new NullReferenceException("Invalid Control color");
    private bool IsEnabled => IsEnabledFn.Invoke();

    public ControlStateManager(Control control, bool invalidateParent = false)
    {
        EventHandler controlDisposed = default!;
        IsEnabledFn = () => control.Enabled || control.Parent?.Enabled == true;
        GetBoundsFn = () => {
            var bounds = control.Bounds;
            bounds.Inflate(3, 3);
            return bounds;
        };
        SubjectState = new BehaviorSubject<State>(State.Normal);
        controlDisposed = (sender, e) => {
            Token.Dispose();
            SubjectState.Dispose();
            control.GotFocus -= Control_GotFocus;
            control.LostFocus -= Control_LostFocus;
            control.MouseEnter -= Control_MouseEnter;
            control.MouseLeave -= Control_MouseLeave;
            control.MouseClick -= Control_MouseClick;
            control.MouseUp -= Control_MouseUp;
            control.MouseDown -= Control_MouseDown;
            control.Disposed -= controlDisposed;
        };

        ThemeType = ThemeType.Default;
        InvalidateParent = invalidateParent;
        control.GotFocus += Control_GotFocus;
        control.LostFocus += Control_LostFocus;
        control.MouseEnter += Control_MouseEnter;
        control.MouseLeave += Control_MouseLeave;
        control.MouseClick += Control_MouseClick;
        control.MouseUp += Control_MouseUp;
        control.MouseDown += Control_MouseDown;
        control.Disposed += controlDisposed;
        Control = control;

        InitializeCheckStatus(control);
        InitializeParentPaint();

        Subscribe(state => {
            TransitionToken.Dispose();
            TransitionToken = SToken.Create();
            control.CrossThread(() => {
                if (InvalidateParent && control.Parent != null)
                    control.Parent.Invalidate(Bounds);
                control.Invalidate();
            });
        }, Token);
    }

    public ControlStateManager(ToolStripItem item, bool invalidateParent = false)
    {
        EventHandler handler = default!;
        ToolStripItem = item;
        IsEnabledFn = () => item.Enabled || item.GetCurrentParent()?.Enabled == true;
        GetBoundsFn = () => {
            var bounds = item.Bounds.Copy();
            bounds.Inflate(3, 3);
            return bounds;
        };
        SubjectState = new BehaviorSubject<State>(State.Normal);
        handler = new EventHandler((sender, e) => {
            Token.Dispose();
            SubjectState.Dispose();
            item.MouseEnter -= Control_MouseEnter;
            item.MouseLeave -= Control_MouseLeave;
            item.Click -= Control_MouseClick;
            item.MouseUp -= Control_MouseUp;
            item.MouseDown -= Control_MouseDown;
            item.Disposed -= handler;
        });

        ThemeType = ThemeType.Default;
        InvalidateParent = invalidateParent;
        item.MouseEnter += Control_MouseEnter;
        item.MouseLeave += Control_MouseLeave;
        item.Click += Control_MouseClick;
        item.MouseUp += Control_MouseUp;
        item.MouseDown += Control_MouseDown;
        item.Disposed += handler;
    }

    public Brush BackBrush(Rectangle viewport, bool buttonLike = false)
    {
        var color = CBackColor;
        if (buttonLike)
        {
            if (State == State.HLite)
                color = color.DarkenOnLightLerp(0.025f);
            else if (State == State.Pressed)
                color = color.Darken(0.1f);
        }
        color = IsEnabled ? color : color.ToGray();
        return IsEnabled && State == State.HLite ? new LinearGradientBrush(viewport, color, color.Darken(0.08f), 90f) : new SolidBrush(color);
    }

    public Color BackColor(bool buttonLike = false)
    {
        var color = CBackColor;
        if (buttonLike)
        {
            if (State == State.HLite)
                color = color.DarkenOnLightLerp(0.1f);
            else if (State == State.Pressed)
                color = color.Darken(0.1f);
        }
        return IsEnabled ? color : color.ToGray();
    }

    public Color BorderColor(bool usePrimaryOnDefaultTheme = true)
    {
        var color = CBackColor;
        if (usePrimaryOnDefaultTheme && ThemeType == ThemeType.Default && (State == State.HLite || IsChecked || IsFocused))
        {
            color = CBackColor;
            if (State == State.HLite)
                color = color.Lighten(0.05f);
        }
        return IsEnabled ? color : color.DarkenOnLightLerp(0.05f);
    }

    public Color ForeColor()
    {
        var foreColor = CForeColor;
        return IsEnabled ? foreColor : foreColor.DarkenOnLightLerp(0.15f);
    }

    public void SetFocus(bool isFocus)
    {
        IsFocused = isFocus;
        SetState(isFocus ? State : State.Normal);
    }

    public void SetState(State nextState)
    {
        if (!IsEnabled && nextState is State.Pressed or State.HLite)
            return;

        SubjectState.OnNext(nextState);
    }

    public void Subscribe(Action<State> callback, SToken token) => SubjectState.Distinct().Subscribe(callback, token);

    private void Control_CheckChanged(object? sender, EventArgs e)
    {
        if (sender is Control control)
        {
            IsChecked = (bool)Checked.GetValue(control)!;
            control.Invalidate();
        }
    }

    private void Control_GotFocus(object? sender, EventArgs e) => SetFocus(true);

    private void Control_LostFocus(object? sender, EventArgs e) => SetFocus(false);

    private void Control_MouseClick(object? sender, MouseEventArgs e) => SetState(State.Pressed);

    private void Control_MouseClick(object? sender, EventArgs e) => SetState(State.Pressed);

    private void Control_MouseDown(object? sender, MouseEventArgs e)
    {
        if (State != State.Pressed)
            SetState(State.Pressed);
    }

    private void Control_MouseEnter(object? sender, EventArgs e) => SetState(State.HLite);

    private void Control_MouseLeave(object? sender, EventArgs e) => SetState(State.Normal);

    private void Control_MouseUp(object? sender, MouseEventArgs e) => SetState(State.HLite);

    private void InitializeCheckStatus(Control control)
    {
        var eventInfo = control.GetType().GetEvent("CheckedChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (eventInfo != null)
        {
            var handler = typeof(ControlStateManager).GetMethod(nameof(Control_CheckChanged), BindingFlags.NonPublic | BindingFlags.Instance)!;
            var callback = Delegate.CreateDelegate(eventInfo.EventHandlerType!, this, handler);
            Checked = MetaType.GetMetadata(control.GetType(), nameof(Checked));

            var addMethod = eventInfo.GetAddMethod();
            addMethod?.Invoke(control, [callback]);
        }
    }

    private void InitializeParentPaint()
    {
        if (!InvalidateParent)
            return;
    }
}

[Flags]
public enum State
{
    Normal = 0x01,
    HLite = 0x02,
    Pressed = 0x04
}