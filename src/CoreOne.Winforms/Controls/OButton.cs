using CoreOne.Drawing;
using CoreOne.Winforms.Services;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace CoreOne.Winforms.Controls;

public class OButton : Control, IButtonControl
{
    private enum HoverRegion
    {
        None,
        Main,
        Chevron
    }

    private readonly LoadingCircle Loading;
    private readonly ControlStateManager StateManager;
    private readonly SToken Token;
    private HoverRegion CurrentHoverRegion = HoverRegion.None;
    [DefaultValue(0)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public int Border { get; set; }
    [DefaultValue(0)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public int BorderRadius {
        get;
        set {
            field = value.Bounds(0, int.MaxValue);
            Invalidate();
        }
    }
    [Browsable(false)]
    public InvokeTask? Clicked { get; set; }
    public DialogResult DialogResult { get; set; }
    [DefaultValue(20)]
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public int SplitWidth {
        get;
        set { field = value; Invalidate(); }
    } = 24;
    [RefreshProperties(RefreshProperties.Repaint)]
    public ThemeType ThemeType {
        get => StateManager.ThemeType;
        set => StateManager.ThemeType = value;
    }
    [DefaultValue(null)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public Image? Image {
        get;
        set { field = value; Invalidate(); }
    }
    [DefaultValue(ContentAlignment.MiddleCenter)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public ContentAlignment ImageAlign { get; set; } = ContentAlignment.MiddleCenter;
    [DefaultValue(typeof(Size), "0, 0")]
    [RefreshProperties(RefreshProperties.Repaint)]
    public Size ImageSize {
        get;
        set { field = value; Invalidate(); }
    } = Size.Empty;
    [DefaultValue(TextImageRelation.ImageBeforeText)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public TextImageRelation TextImageRelation { get; set; } = TextImageRelation.ImageBeforeText;
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

    public void PerformClick() => OnClick(EventArgs.Empty);

    protected override void Dispose(bool disposing)
    {
        Token.Dispose();
        base.Dispose(disposing);
    }

    protected override void OnClick(EventArgs e)
    {
        if (e is MouseEventArgs me && IsMenuClicked(me))
        {// If the click is from the dropdown area, show the context menu instead of performing the button click action
            return;
        }

        if (Clicked != null)
        {
            Loading.InvokeAsync(Clicked, Token);
        }
        else
        {
            base.OnClick(EventArgs.Empty);
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (!IsMenuClicked(e))
        {
            Focus();
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

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (e is MouseEventArgs me)
        {
            UpdateHoverRegion(me.Location);
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        CurrentHoverRegion = HoverRegion.None;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        UpdateHoverRegion(e.Location);
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
        bool hasChevron = ContextMenuStrip?.Items.Count > 0 && SplitWidth > 0;
        var contentArea = hasChevron
            ? new Rectangle(viewport.X, viewport.Y, viewport.Width - SplitWidth, viewport.Height)
            : viewport;
        RenderImageAndText(graphics, contentArea, foreColor);
        if (hasChevron)
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
        var viewport = new Rectangle(
                ClientRectangle.X + Padding.Left,
                ClientRectangle.Y + Padding.Top,
                ClientRectangle.Width - Padding.Horizontal,
                ClientRectangle.Height - Padding.Vertical);
        var backColor = StateManager.BackColor(true);
        var borderColor = StateManager.BorderColor(checkd);

        viewport.Inflate(-1, -1);
        using var path = Drawings.RoundRect(viewport, BorderRadius);
        using var brush = StateManager.BackBrush(viewport, true);
        g.Clear(Parent?.BackColor ?? backColor);
        g.FillPath(brush, path);
        if (BorderRadius > 0)
        {
            g.FillPath(new SolidBrush(backColor), path);
        }
        if (Border > 0)
        {
            using var pen = new Pen(borderColor, Border);
            g.DrawPath(pen, path);
        }

        g.SetClip(path, CombineMode.Intersect);
        if (StateManager.State == State.HLite)
        {
            // Determine which region to highlight
            Rectangle highlightRect = viewport;
            bool hasChevron = ContextMenuStrip?.Items.Count > 0 && SplitWidth > 0;
            if (hasChevron)
            {
                // Only highlight the specific region where the mouse is
                if (CurrentHoverRegion == HoverRegion.Main)
                {
                    // Highlight only the main button area (left side)
                    highlightRect = new Rectangle(viewport.X, viewport.Y, viewport.Width - SplitWidth, viewport.Height);
                }
                else if (CurrentHoverRegion == HoverRegion.Chevron)
                {
                    // Highlight only the chevron area (right side)
                    highlightRect = new Rectangle(viewport.X + viewport.Width - SplitWidth, viewport.Y, SplitWidth, viewport.Height);
                }
                else
                {
                    // No hover, don't highlight
                    g.ResetClip();
                    return;
                }
            }

            // Create a clip region for the highlight area
            using var highlightPath = Drawings.RoundRect(highlightRect, BorderRadius);
            g.SetClip(highlightPath, CombineMode.Intersect);

            var pressed = StateManager.State == State.Pressed;
            var glowColor = backColor.DarkenOnLightLerp(0.2f).SetAlpha(180);
            var fillNorth = pressed ? Color.FromArgb(150, backColor) : glowColor.SetAlpha(60);
            var fillSouth = pressed ? Color.FromArgb(10, backColor) : glowColor.SetAlpha(1);
            using (Brush b = new LinearGradientBrush(highlightRect, fillNorth, fillSouth, LinearGradientMode.Vertical))
            {
                g.FillRectangle(b, highlightRect);
            }

            var bv = highlightRect;
            using GraphicsPath brad = CreateBottomRadialPath(bv);
            using PathGradientBrush pgr = new PathGradientBrush(brad);
            var bounds = brad.GetBounds();
            pgr.CenterPoint = new PointF((bounds.Left + bounds.Right) / 2f, (bounds.Top + bounds.Bottom) / 2f);
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

    private const int ImageTextGap = 4;

    private static Rectangle AlignRect(Size size, Rectangle container, ContentAlignment align)
    {
        int x = align switch {
            ContentAlignment.TopLeft or ContentAlignment.MiddleLeft or ContentAlignment.BottomLeft
                => container.X,
            ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight
                => container.Right - size.Width,
            _ => container.X + (container.Width - size.Width) / 2
        };
        int y = align switch {
            ContentAlignment.TopLeft or ContentAlignment.TopCenter or ContentAlignment.TopRight
                => container.Y,
            ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight
                => container.Bottom - size.Height,
            _ => container.Y + (container.Height - size.Height) / 2
        };
        return new Rectangle(x, y, size.Width, size.Height);
    }

    private void RenderImageAndText(Graphics g, Rectangle contentArea, Color foreColor)
    {
        using var brush = new SolidBrush(foreColor);

        Size scaledImageSize = Size.Empty;
        if (Image != null && contentArea.Height > 0)
        {
            float scale;
            if (!ImageSize.IsEmpty)
            {
                scale = Math.Min((float)ImageSize.Width / Image.Width, (float)ImageSize.Height / Image.Height);
            }
            else
            {
                scale = (float)contentArea.Height / Image.Height;
            }
            scaledImageSize = new Size(Math.Max(1, (int)(Image.Width * scale)), Math.Max(1, (int)(Image.Height * scale)));
        }

        bool hasImage = Image != null && scaledImageSize != Size.Empty;
        bool hasText = !string.IsNullOrEmpty(Text);

        if (!hasImage)
        {
            g.DrawString(Text, Font, brush, contentArea, StringAlign.Center);
            return;
        }

        if (!hasText)
        {
            var imgRect = AlignRect(scaledImageSize, contentArea, ImageAlign);
            g.DrawImage(Image!, imgRect);
            return;
        }

        if (TextImageRelation == TextImageRelation.Overlay)
        {
            var imgRect = AlignRect(scaledImageSize, contentArea, ImageAlign);
            g.DrawImage(Image!, imgRect);
            g.DrawString(Text, Font, brush, contentArea, StringAlign.Center);
            return;
        }

        var textSize = g.MeasureString(Text, Font);
        int textW = (int)Math.Ceiling(textSize.Width);
        int textH = (int)Math.Ceiling(textSize.Height);
        int imgW = scaledImageSize.Width;
        int imgH = scaledImageSize.Height;

        Rectangle blockRect;
        Rectangle imageRectInBlock;
        Rectangle textRectInBlock;

        switch (TextImageRelation)
        {
            case TextImageRelation.ImageBeforeText:
            case TextImageRelation.TextBeforeImage:
                {
                    int blockW = imgW + ImageTextGap + textW;
                    int blockH = Math.Max(imgH, textH);
                    blockRect = AlignRect(new Size(blockW, blockH), contentArea, ImageAlign);
                    int imgY = blockRect.Y + (blockH - imgH) / 2;
                    int txtY = blockRect.Y + (blockH - textH) / 2;
                    if (TextImageRelation == TextImageRelation.ImageBeforeText)
                    {
                        imageRectInBlock = new Rectangle(blockRect.X, imgY, imgW, imgH);
                        textRectInBlock = new Rectangle(blockRect.X + imgW + ImageTextGap, txtY, textW, textH);
                    }
                    else
                    {
                        textRectInBlock = new Rectangle(blockRect.X, txtY, textW, textH);
                        imageRectInBlock = new Rectangle(blockRect.X + textW + ImageTextGap, imgY, imgW, imgH);
                    }
                    break;
                }
            case TextImageRelation.ImageAboveText:
            case TextImageRelation.TextAboveImage:
                {
                    int blockW = Math.Max(imgW, textW);
                    int blockH = imgH + ImageTextGap + textH;
                    blockRect = AlignRect(new Size(blockW, blockH), contentArea, ImageAlign);
                    int imgX = blockRect.X + (blockW - imgW) / 2;
                    int txtX = blockRect.X + (blockW - textW) / 2;
                    if (TextImageRelation == TextImageRelation.ImageAboveText)
                    {
                        imageRectInBlock = new Rectangle(imgX, blockRect.Y, imgW, imgH);
                        textRectInBlock = new Rectangle(txtX, blockRect.Y + imgH + ImageTextGap, textW, textH);
                    }
                    else
                    {
                        textRectInBlock = new Rectangle(txtX, blockRect.Y, textW, textH);
                        imageRectInBlock = new Rectangle(imgX, blockRect.Y + textH + ImageTextGap, imgW, imgH);
                    }
                    break;
                }
            default:
                g.DrawString(Text, Font, brush, contentArea, StringAlign.Center);
                return;
        }

        g.DrawImage(Image!, imageRectInBlock);
        g.DrawString(Text, Font, brush, textRectInBlock, StringAlign.Center);
    }

    private void UpdateHoverRegion(Point location)
    {
        var newRegion = HoverRegion.Main;
        if (ContextMenuStrip?.Items.Count > 0 && SplitWidth > 0)
        {
            var splitView = SplitView;
            newRegion = splitView.Contains(location) ? HoverRegion.Chevron : HoverRegion.Main;
        }

        if (CurrentHoverRegion != newRegion)
        {
            CurrentHoverRegion = newRegion;
            Invalidate();
        }
    }
}