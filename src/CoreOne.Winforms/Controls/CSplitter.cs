using System.Drawing.Drawing2D;

namespace CoreOne.Winforms.Controls;

public class CSplitter : SplitContainer
{
    private Color PHandleColor;

    public Color HandleColor {
        get => PHandleColor;
        set {
            PHandleColor = value;
            Transparent = Color.FromArgb(0, value);
            Invalidate();
        }
    }

    public Color Transparent { get; private set; }

    public CSplitter()
    {
        Paint += CSplitter_Paint;
    }

    public float GetAngle() => Orientation == Orientation.Horizontal ? 180 : 90;

    public Rectangle GetSplitterBounds() => Orientation == Orientation.Horizontal ?
            new Rectangle(0, SplitterDistance, Width, SplitterWidth) :
            new Rectangle(SplitterDistance, 0, SplitterWidth, Height);

    private void CSplitter_Paint(object? sender, PaintEventArgs e)
    {
        LinearGradientBrush brush;
        Rectangle
            splitterarea,
            drawingarea;

        //Color highlight;
        var blend = new ColorBlend {
            Colors = [Transparent, HandleColor, Transparent],
            Positions = [0, 0.5F, 1]
        };
        float angle = GetAngle();
        splitterarea = GetSplitterBounds();
        if (Orientation == Orientation.Horizontal)
        {
            drawingarea = splitterarea;
            drawingarea.Inflate(Convert.ToInt32(Width * -0.1F), -1);
        }
        else
        {
            drawingarea = splitterarea;
            drawingarea.Inflate(-1, Convert.ToInt32(Height * -0.1F));
        }
        using (brush = new LinearGradientBrush(drawingarea, HandleColor, Transparent, angle))
        {
            brush.InterpolationColors = blend;
            e.Graphics.FillRectangle(brush, drawingarea);
        }
    }
}