using CoreOne.Threading.Tasks;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace CoreOne.Winforms.Controls;

public class LoadingCircle : Control
{
    private record CircleDefinition(int InnerCircleRadius, int NumberOfSpokes, int OuterCircleRadius, int SpokeThickness);
    private readonly CircleDefinition
        Default = new(8, 10, 10, 4),
        FireFox = new(6, 9, 7, 4),
        IE7 = new(8, 24, 9, 4),
        MacOSX = new(5, 12, 11, 2);

    private const double NumberOfDegreesInCircle = 360;
    private const double NumberOfDegreesInHalfCircle = NumberOfDegreesInCircle / 2;
    private readonly IContainer? components = null;
    private readonly Color DefaultColor = Color.DarkGray;
    private readonly LoadingStore LoadingStore = new();
    private readonly Timer m_Timer;
    private readonly SToken Token;
    private double[] m_Angles = [];
    private PointF m_CenterPoint;
    private Color m_Color;
    private Color[] m_Colors = [];
    private int m_InnerCircleRadius;
    private bool m_IsTimerActive;
    private int m_NumberOfSpoke;
    private int m_OuterCircleRadius;
    private int m_ProgressValue;
    private int m_SpokeThickness;
    private StylePresets m_StylePreset;
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:LoadingCircle"/> is active.
    /// </summary>
    /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
    [Description("Gets or sets the number of spoke."),
    Category("LoadingCircle")]
    public bool Active {
        get => m_IsTimerActive;
        set {
            m_IsTimerActive = value;
            ActiveTimer();
        }
    }

    // Properties ========================================================
    /// <summary>
    /// Gets or sets the lightest color of the circle.
    /// </summary>
    /// <value>The lightest color of the circle.</value>
    [TypeConverter("System.Drawing.ColorConverter"),
     Category("LoadingCircle"),
     Description("Sets the color of spoke.")]
    public Color Color {
        get => m_Color;
        set {
            m_Color = value;

            GenerateColorsPallet();
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the inner circle radius.
    /// </summary>
    /// <value>The inner circle radius.</value>
    [Description("Gets or sets the radius of inner circle."),
     Category("LoadingCircle")]
    public int InnerCircleRadius {
        get {
            if (m_InnerCircleRadius == 0)
                m_InnerCircleRadius = Default.InnerCircleRadius;

            return m_InnerCircleRadius;
        }
        set {
            m_InnerCircleRadius = value;
            Invalidate();
        }
    }

    [Browsable(false)] public bool IsBusy => LoadingStore.IsBusy || Active;
    /// <summary>
    /// Gets or sets the number of spoke.
    /// </summary>
    /// <value>The number of spoke.</value>
    [Description("Gets or sets the number of spoke."),
    Category("LoadingCircle")]
    public int NumberSpoke {
        get {
            if (m_NumberOfSpoke == 0)
                m_NumberOfSpoke = Default.NumberOfSpokes;

            return m_NumberOfSpoke;
        }
        set {
            if (m_NumberOfSpoke != value && m_NumberOfSpoke > 0)
            {
                m_NumberOfSpoke = value;
                GenerateColorsPallet();
                GetSpokesAngles();

                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the outer circle radius.
    /// </summary>
    /// <value>The outer circle radius.</value>
    [Description("Gets or sets the radius of outer circle."),
     Category("LoadingCircle")]
    public int OuterCircleRadius {
        get {
            if (m_OuterCircleRadius == 0)
                m_OuterCircleRadius = Default.OuterCircleRadius;

            return m_OuterCircleRadius;
        }
        set {
            m_OuterCircleRadius = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the rotation speed.
    /// </summary>
    /// <value>The rotation speed.</value>
    [Description("Gets or sets the rotation speed. Higher the slower."),
    Category("LoadingCircle")]
    public int RotationSpeed {
        get => m_Timer.Interval;
        set {
            if (value > 0)
                m_Timer.Interval = value;
        }
    }

    /// <summary>
    /// Gets or sets the spoke thickness.
    /// </summary>
    /// <value>The spoke thickness.</value>
    [Description("Gets or sets the thickness of a spoke."),
    Category("LoadingCircle")]
    public int SpokeThickness {
        get {
            if (m_SpokeThickness <= 0)
                m_SpokeThickness = Default.SpokeThickness;

            return m_SpokeThickness;
        }
        set {
            m_SpokeThickness = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Quickly sets the style to one of these presets, or a custom style if desired
    /// </summary>
    /// <value>The style preset.</value>
    [Category("LoadingCircle"),
     Description("Quickly sets the style to one of these presets, or a custom style if desired"),
     DefaultValue(typeof(StylePresets), "Custom")]
    public StylePresets StylePreset {
        get => m_StylePreset;
        set {
            m_StylePreset = value;
            var style = value switch {
                StylePresets.MacOSX => MacOSX,
                StylePresets.Firefox => FireFox,
                StylePresets.IE7 => IE7,
                _ => Default
            };
            SetCircleAppearance(style.NumberOfSpokes, style.SpokeThickness, style.InnerCircleRadius, style.OuterCircleRadius);
        }
    }

    [Browsable(false)]
    public Action? Tick { get; set; }

    // Construtor ========================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="T:LoadingCircle"/> class.
    /// </summary>
    public LoadingCircle()
    {
        Token = this.CreateSToken();
        LoadingStore.Subscribe(p => {
            Active = p;
            if (Parent is not null)
            {
                Parent.Enabled = p;
                Parent.Invalidate();
            }
        }, Token);
        SetStyle(ControlStyles.UserPaint, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        SetStyle(ControlStyles.ResizeRedraw, true);
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);

        m_Color = DefaultColor;

        GenerateColorsPallet();
        GetSpokesAngles();
        GetControlCenterPoint();

        m_Timer = new Timer();
        m_Timer.Tick += Timer_Tick;
        ActiveTimer();
    }

    /// <summary>
    /// Gets the control center point.
    /// </summary>
    /// <returns>PointF object</returns>
    public void GetControlCenterPoint() => m_CenterPoint = new PointF(Width / 2F, (Height / 2F) - 1);

    // Overridden Methods ================================================
    /// <summary>
    /// Retrieves the size of a rectangular area into which a control can be fitted.
    /// </summary>
    /// <param name="proposedSize">The custom-sized area for a control.</param>
    /// <returns>
    /// An ordered pair of type <see cref="T:System.Drawing.Size"></see> representing the width and height of a rectangle.
    /// </returns>
    public override Size GetPreferredSize(Size proposedSize)
    {
        proposedSize.Width = (m_OuterCircleRadius + m_SpokeThickness) * 2;
        return proposedSize;
    }

    public Task<TResult?> GetResultAsync<TResult>(InvokeTask<TResult> callback, CancellationToken cancellationToken = default) => LoadingStore.GetResultAsync(callback, null, cancellationToken);

    public Task InvokeAsync(InvokeTask? callback, CancellationToken cancellationToken = default) => LoadingStore.InvokeAsync(new InvokeCallback(callback), cancellationToken);

    public void PaintSpinner(Graphics graphics)
    {
        graphics.SmoothingMode = SmoothingMode.HighQuality;

        int intPosition = m_ProgressValue;
        for (int intCounter = 0; intCounter < m_NumberOfSpoke; intCounter++)
        {
            intPosition %= m_NumberOfSpoke;
            DrawLine(graphics,
                     GetCoordinate(m_CenterPoint, m_InnerCircleRadius, m_Angles[intPosition]),
                     GetCoordinate(m_CenterPoint, m_OuterCircleRadius, m_Angles[intPosition]),
                     m_Colors[intCounter], m_SpokeThickness);
            intPosition++;
        }
    }

    /// <summary>
    /// Sets the circle appearance.
    /// </summary>
    /// <param name="numberSpoke">The number spoke.</param>
    /// <param name="spokeThickness">The spoke thickness.</param>
    /// <param name="innerCircleRadius">The inner circle radius.</param>
    /// <param name="outerCircleRadius">The outer circle radius.</param>
    public void SetCircleAppearance(int numberSpoke, int spokeThickness, int innerCircleRadius, int outerCircleRadius)
    {
        NumberSpoke = numberSpoke;
        SpokeThickness = spokeThickness;
        InnerCircleRadius = innerCircleRadius;
        OuterCircleRadius = outerCircleRadius;

        Invalidate();
    }

    public void Subscribe(Action<bool> callback, SToken token) => LoadingStore.Subscribe(callback, token);

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data.</param>
    protected override void OnPaint(PaintEventArgs e)
    {
        if (m_NumberOfSpoke > 0)
        {
            PaintSpinner(e.Graphics);
        }

        base.OnPaint(e);
    }

    protected override void OnResize(EventArgs e) => GetControlCenterPoint();

    // Methods ===========================================================
    /// <summary>
    /// Darkens a specified color.
    /// </summary>
    /// <param name="_objColor">Color to darken.</param>
    /// <param name="_intPercent">The percent of darken.</param>
    /// <returns>The new color generated.</returns>
    private static Color Darken(Color _objColor, int _intPercent)
    {
        int intRed = _objColor.R;
        int intGreen = _objColor.G;
        int intBlue = _objColor.B;
        return Color.FromArgb(_intPercent, Math.Min(intRed, byte.MaxValue), Math.Min(intGreen, byte.MaxValue), Math.Min(intBlue, byte.MaxValue));
    }

    /// <summary>
    /// Draws the line with GDI+.
    /// </summary>
    /// <param name="_objGraphics">The Graphics object.</param>
    /// <param name="_objPointOne">The point one.</param>
    /// <param name="_objPointTwo">The point two.</param>
    /// <param name="_objColor">Color of the spoke.</param>
    /// <param name="_intLineThickness">The thickness of spoke.</param>
    private static void DrawLine(Graphics _objGraphics, PointF _objPointOne, PointF _objPointTwo, Color _objColor, int _intLineThickness)
    {
        using var objPen = new Pen(new SolidBrush(_objColor), _intLineThickness) {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        _objGraphics.DrawLine(objPen, _objPointOne, _objPointTwo);
    }

    /// <summary>
    /// Gets the coordinate.
    /// </summary>
    /// <param name="_objCircleCenter">The Circle center.</param>
    /// <param name="_intRadius">The radius.</param>
    /// <param name="_dblAngle">The angle.</param>
    /// <returns></returns>
    private static PointF GetCoordinate(PointF _objCircleCenter, int _intRadius, double _dblAngle)
    {
        double dblAngle = Math.PI * _dblAngle / NumberOfDegreesInHalfCircle;
        return new PointF(_objCircleCenter.X + (_intRadius * (float)Math.Cos(dblAngle)),
                          _objCircleCenter.Y + (_intRadius * (float)Math.Sin(dblAngle)));
    }

    /// <summary>
    /// Gets the spoke angles.
    /// </summary>
    /// <param name="_intNumberSpoke">The number spoke.</param>
    /// <returns>An array of angle.</returns>
    private static double[] GetSpokesAngles(int _intNumberSpoke)
    {
        double[] Angles = new double[_intNumberSpoke];
        double dblAngle = NumberOfDegreesInCircle / _intNumberSpoke;

        for (int shtCounter = 0; shtCounter < _intNumberSpoke; shtCounter++)
            Angles[shtCounter] = shtCounter == 0 ? dblAngle : Angles[shtCounter - 1] + dblAngle;

        return Angles;
    }

    /// <summary>
    /// Actives the timer.
    /// </summary>
    private void ActiveTimer()
    {
        if (m_IsTimerActive)
            m_Timer.Start();
        else
        {
            m_Timer.Stop();
            m_ProgressValue = 0;
        }

        GenerateColorsPallet();
        Invalidate();
    }

    /// <summary>
    /// Generates the colors pallet.
    /// </summary>
    private void GenerateColorsPallet() => m_Colors = GenerateColorsPallet(m_Color, Active, m_NumberOfSpoke);

    /// <summary>
    /// Generates the colors pallet.
    /// </summary>
    /// <param name="_objColor">Color of the lightest spoke.</param>
    /// <param name="_blnShadeColor">if set to <c>true</c> the color will be shaded on X spoke.</param>
    /// <param name="_intNbSpoke">The number spoke.</param>
    /// <returns>An array of color used to draw the circle.</returns>
    private Color[] GenerateColorsPallet(Color _objColor, bool _blnShadeColor, int _intNbSpoke)
    {
        Color[] objColors = new Color[NumberSpoke];

        // Value is used to simulate a gradient feel... For each spoke, the
        // color will be darken by value in intIncrement.
        byte bytIncrement = (byte)(byte.MaxValue / NumberSpoke);

        //Reset variable in case of multiple passes
        byte darkenPercentage = 0;
        for (int intCursor = 0; intCursor < NumberSpoke; intCursor++)
        {
            if (_blnShadeColor)
            {
                if (intCursor == 0 || intCursor < NumberSpoke - _intNbSpoke)
                    objColors[intCursor] = _objColor;
                else
                {
                    // Increment alpha channel color
                    darkenPercentage += bytIncrement;

                    // Ensure that we don't exceed the maximum alpha
                    // channel value (255)
                    if (darkenPercentage > byte.MaxValue)
                        darkenPercentage = byte.MaxValue;

                    // Determine the spoke forecolor
                    objColors[intCursor] = Darken(_objColor, darkenPercentage);
                }
            }
            else
                objColors[intCursor] = _objColor;
        }

        return objColors;
    }

    /// <summary>
    /// Gets the spokes angles.
    /// </summary>
    private void GetSpokesAngles() => m_Angles = GetSpokesAngles(NumberSpoke);

    /// <summary>
    /// Handles the Tick event of the aTimer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        var callback = Tick ?? Invalidate;
        m_ProgressValue = ++m_ProgressValue % m_NumberOfSpoke;
        callback.Invoke();
    }
}