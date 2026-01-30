using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace CoreOne.Winforms.Controls;

/// <summary>
/// A control that displays a star rating (1-5 stars) for numeric values
/// </summary>
public class RatingControl : Control
{
    private int _value = 0;
    private int _maxRating = 5;
    private int _hoveredStar = -1;
    private int _starSize = 24;
    private int _starSpacing = 4;
    private Color _fillColor = Color.Gold;
    private Color _emptyColor = Color.LightGray;
    private Color _hoverColor = Color.Orange;
    private bool _readOnly = false;

    public event EventHandler? ValueChanged;

    /// <summary>
    /// Gets or sets the current rating value (0 to MaxRating)
    /// </summary>
    [DefaultValue(0)]
    public int Value {
        get => _value;
        set {
            if (_value != value && value >= 0 && value <= _maxRating)
            {
                _value = value;
                OnValueChanged();
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum rating (default is 5)
    /// </summary>
    [DefaultValue(5)]
    public int MaxRating {
        get => _maxRating;
        set {
            if (_maxRating != value && value > 0)
            {
                _maxRating = value;
                if (_value > _maxRating)
                    Value = _maxRating;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the size of each star in pixels
    /// </summary>
    [DefaultValue(24)]
    public int StarSize {
        get => _starSize;
        set {
            if (_starSize != value && value > 0)
            {
                _starSize = value;
                UpdateSize();
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the spacing between stars in pixels
    /// </summary>
    [DefaultValue(4)]
    public int StarSpacing {
        get => _starSpacing;
        set {
            if (_starSpacing != value && value >= 0)
            {
                _starSpacing = value;
                UpdateSize();
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the color of filled stars
    /// </summary>
    public Color FillColor {
        get => _fillColor;
        set {
            if (_fillColor != value)
            {
                _fillColor = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the color of empty stars
    /// </summary>
    public Color EmptyColor {
        get => _emptyColor;
        set {
            if (_emptyColor != value)
            {
                _emptyColor = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the color of stars on hover
    /// </summary>
    public Color HoverColor {
        get => _hoverColor;
        set {
            if (_hoverColor != value)
            {
                _hoverColor = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the control is read-only
    /// </summary>
    [DefaultValue(false)]
    public bool ReadOnly {
        get => _readOnly;
        set {
            if (_readOnly != value)
            {
                _readOnly = value;
                Cursor = value ? Cursors.Default : Cursors.Hand;
            }
        }
    }

    public RatingControl()
    {
        SetStyle(ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);

        Cursor = Cursors.Hand;
        UpdateSize();
    }

    protected virtual void OnValueChanged()
    {
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateSize()
    {
        var totalWidth = (_starSize * _maxRating) + (_starSpacing * (_maxRating - 1));
        Size = new Size(totalWidth, _starSize);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        for (int i = 0; i < _maxRating; i++)
        {
            var x = i * (_starSize + _starSpacing);
            var rect = new Rectangle(x, 0, _starSize, _starSize);

            // Determine the color for this star
            Color starColor;
            if (!_readOnly && _hoveredStar >= 0 && i <= _hoveredStar)
            {
                starColor = _hoverColor;
            }
            else if (i < _value)
            {
                starColor = _fillColor;
            }
            else
            {
                starColor = _emptyColor;
            }

            DrawStar(g, rect, starColor);
        }
    }

    private void DrawStar(Graphics g, Rectangle bounds, Color color)
    {
        using var brush = new SolidBrush(color);
        using var pen = new Pen(Color.FromArgb(Math.Max(0, color.R - 40),
                                                Math.Max(0, color.G - 40),
                                                Math.Max(0, color.B - 40)), 1.5f);

        var points = GetStarPoints(bounds);
        g.FillPolygon(brush, points);
        g.DrawPolygon(pen, points);
    }

    private PointF[] GetStarPoints(Rectangle bounds)
    {
        var centerX = bounds.Left + bounds.Width / 2f;
        var centerY = bounds.Top + bounds.Height / 2f;
        var outerRadius = Math.Min(bounds.Width, bounds.Height) / 2f * 0.95f;
        var innerRadius = outerRadius * 0.4f;

        var points = new PointF[10];
        var angle = -Math.PI / 2; // Start from top

        for (int i = 0; i < 10; i++)
        {
            var radius = i % 2 == 0 ? outerRadius : innerRadius;
            points[i] = new PointF(
                centerX + (float)(radius * Math.Cos(angle)),
                centerY + (float)(radius * Math.Sin(angle))
            );
            angle += Math.PI / 5; // 36 degrees
        }

        return points;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_readOnly)
            return;

        var starIndex = GetStarAtPosition(e.Location);
        if (_hoveredStar != starIndex)
        {
            _hoveredStar = starIndex;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (_hoveredStar != -1)
        {
            _hoveredStar = -1;
            Invalidate();
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        if (_readOnly)
            return;

        var starIndex = GetStarAtPosition(e.Location);
        if (starIndex >= 0)
        {
            Value = starIndex + 1;
        }
    }

    private int GetStarAtPosition(Point location)
    {
        if (location.Y < 0 || location.Y > _starSize)
            return -1;

        for (int i = 0; i < _maxRating; i++)
        {
            var x = i * (_starSize + _starSpacing);
            if (location.X >= x && location.X <= x + _starSize)
            {
                return i;
            }
        }

        return -1;
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }
}