using CoreOne.Drawing;
using CoreOne.Winforms.Services;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace CoreOne.Winforms.Controls;

public class OButton : Control, IButtonControl
{
    public event EventHandler? ButtonClick;
    private readonly LoadingCircle Loading;
    private readonly ControlStateManager StateManager;
    private readonly SToken Token;
    private int _SplitWidth = 24;
    [DefaultValue(0)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public int Border { get; set; }
    [Browsable(false)]
    public InvokeTask? Clicked { get; set; }
    public DialogResult DialogResult { get; set; }
    [DefaultValue(20)]
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public int SplitWidth {
        get => _SplitWidth;
        set { _SplitWidth = value; Invalidate(); }
    }
    [RefreshProperties(RefreshProperties.Repaint)]
    public ThemeType ThemeType {
        get => StateManager.ThemeType;
        set => StateManager.ThemeType = value;
    }
    protected Rectangle SplitView => new(Width - SplitWidth, 0, SplitWidth, Height);

    public OButton()
    {
        var styles = ControlStyles.Opaque |
            ControlStyles.ResizeRedraw |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.SupportsTransparentBackColor;
        SetStyle(styles, true);
        Token = this.CreateSToken();
        StateManager = new ControlStateManager(this);
        this.OnEnterClick(this);

        Loading = new LoadingCircle {
            Tick = Invalidate,
            StylePreset = StylePresets.Firefox
        };
        Loading.Subscribe(p => {
            Enabled = !p;
            Invalidate();
        }, Token);
    }

    public void NotifyDefault(bool value) => Invalidate();

    public void PerformClick()
    {
        OnClick(EventArgs.Empty);
        OnButtonClicked();
    }

    protected override void Dispose(bool disposing)
    {
        Token.Dispose();
        base.Dispose(disposing);
    }

    protected virtual void OnButtonClicked()
    {
        ButtonClick?.Invoke(this, EventArgs.Empty);
        if (Clicked != null)
        {
            Loading.InvokeAsync(Clicked, Token);
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (!IsMenuClicked(e))
        {
            Focus();
            OnButtonClicked();
            MouseEvent(base.OnMouseClick, e);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (!IsMenuClicked(e))
        {
            MouseEvent(base.OnMouseDown, e);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (!IsMenuClicked(e))
        {
            MouseEvent(base.OnMouseUp, e);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        var viewport = DisplayRectangle;
        var foreColor = StateManager.ForeColor();

        graphics.Pretty();
        OnPaintBackground(e);
        if (Loading.IsBusy)
        {
            Loading.PaintSpinner(e.Graphics);
            return;
        }
        graphics.DrawString(Text, Font, new SolidBrush(foreColor), viewport, StringAlign.Center);
        if (ContextMenuStrip?.Items.Count > 0 && SplitWidth > 0)
            RenderDropDown(graphics, foreColor);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        var g = pevent.Graphics;
        RenderBackground(g, StateManager.IsChecked);
    }

    protected override void OnResize(EventArgs e)
    {
        Loading.Size = Size;
        base.OnResize(e);
    }

    private static GraphicsPath CreateBottomRadialPath(Rectangle rectangle)
    {
        GraphicsPath path = new GraphicsPath();
        RectangleF rect = rectangle;
        rect.X -= rect.Width * .35f;
        rect.Y -= rect.Height * .15f;
        rect.Width *= 1.7f;
        rect.Height *= 2.3f;
        path.AddEllipse(rect);
        path.CloseFigure();
        return path;
    }

    private static void MouseEvent<T>(Action<T> action, T e) => action(e);

    private bool IsMenuClicked(MouseEventArgs mevent)
    {
        var pressed = false;
        var view = SplitView;
        if (ContextMenuStrip != null && mevent.Button == MouseButtons.Left && view.Contains(mevent.Location))
        {
            pressed = true;
            ContextMenuStrip.Show(this, 0, Height); // Shows menu under button
        }
        return pressed;
    }

    private void RenderBackground(Graphics g, bool checkd)
    {
        var radius = 4;
        var viewport = DisplayRectangle;
        var backColor = StateManager.BackColor(true);
        var borderColor = StateManager.BorderColor(checkd);

        viewport.Inflate(-1, -1);
        using var path = Drawings.RoundRect(viewport, radius);
        using var brush = StateManager.BackBrush(viewport, true);
        g.Clear(backColor);
        g.FillPath(brush, path);
        if (Border > 0)
        {
            using var pen = new Pen(borderColor, Border);
            g.DrawPath(pen, path);
        }

        g.SetClip(path, CombineMode.Intersect);
        if (StateManager.State == State.HLite)
        {
            var pressed = StateManager.State == State.Pressed;
            var glowColor = backColor.DarkenOnLightLerp(0.15f).SetAlpha(180);
            var fillNorth = pressed ? Color.FromArgb(150, backColor) : glowColor.SetAlpha(60);
            var fillSouth = pressed ? Color.FromArgb(10, backColor) : glowColor.SetAlpha(1);
            using (Brush b = new LinearGradientBrush(viewport, fillNorth, fillSouth, LinearGradientMode.Vertical))
            {
                g.FillPath(b, path);
            }

            var bv = viewport;
            using GraphicsPath brad = CreateBottomRadialPath(bv);
            using PathGradientBrush pgr = new PathGradientBrush(brad);
            var bounds = brad.GetBounds();
            pgr.CenterPoint = new PointF((bounds.Left + bounds.Right) / 2f, bv.Height);
            pgr.CenterColor = glowColor.SetAlpha(200);
            pgr.SurroundColors = [Color.FromArgb(0, glowColor)];
            pgr.FocusScales = new PointF(0, 0);
            g.FillPath(pgr, brad);
        }
        g.ResetClip();
    }

    private void RenderDropDown(Graphics g, Color foreColor)
    { // Draw the arrow glyph on the right side of the button
        int arrowX = ClientRectangle.Width - 14;
        int arrowY = (ClientRectangle.Height / 2) - 1;
        var arrows = new[] { new Point(arrowX, arrowY), new Point(arrowX + 7, arrowY), new Point(arrowX + 3, arrowY + 4) };
        using (var brush = new SolidBrush(foreColor))
        {
            g.FillPolygon(brush, arrows);
        }

        // Draw a dashed separator on the left of the arrow
        int lineX = ClientRectangle.Width - SplitWidth;
        int lineYFrom = (ClientRectangle.Height / 2) - (arrowY / 2);
        int lineYTo = lineYFrom + arrowY;
        using var separatorPen = new Pen(foreColor) { DashStyle = DashStyle.Dot };
        g.DrawLine(separatorPen, lineX, lineYFrom, lineX, lineYTo);
    }
}