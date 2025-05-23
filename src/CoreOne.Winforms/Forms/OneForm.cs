using CoreOne.Drawing;
using CoreOne.Winforms.Controls;
using CoreOne.Winforms.Events;
using CoreOne.Winforms.Models;
using CoreOne.Winforms.Native;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace CoreOne.Winforms.Forms;

[ToolboxItem(false)]
[SupportedOSPlatform("windows")]
public class OneForm : Form, ITheme
{
    private sealed class FlickerFreePanel : Control
    {
        private Color PBackColor = SystemColors.Window;

        public override Color BackColor {
            get => PBackColor;
            set {
                PBackColor = value;
                InitBrush(value);
            }
        }

        public SolidBrush Brush { get; private set; }

        public FlickerFreePanel()
        {
            Visible = false;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            InitBrush(SystemColors.Window);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (BackgroundImage != null)
            {
                e.Graphics.DrawImageUnscaled(BackgroundImage, 0, 0, BackgroundImage.Width, BackgroundImage.Height);
            }
            else
            {
                // No background available (BeginAsyncOperation was probably called during the subclass constructor)...
                // So just fill with the normal back colour instead...
                e.Graphics.FillRectangle(Brush, 0, 0, Width, Height);
            }
        }

        [MemberNotNull(nameof(Brush))]
        private void InitBrush(Color color)
        {
            Brush?.Dispose();
            Brush = new SolidBrush(Color.FromArgb(180, color));
        }
    }

    private readonly LoadingCircle BarberPole = new();
    private readonly FlickerFreePanel PAsyncPanel = new();
    private readonly AsyncTaskQueue Queue = new(4);
    private int ReferenceCount = 0;
    public override Size MaximumSize {
        get => !IsAsyncBusy ? base.MaximumSize : Size;
        set => base.MaximumSize = value;
    }
    public override Size MinimumSize {
        get {
            return !IsAsyncBusy ? base.MinimumSize : Size;
        }
        set => base.MinimumSize = value;
    }
    protected bool IsAsyncBusy => ReferenceCount > 0;
    protected SToken Token { get; } = SToken.Create();

    protected OneForm()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

        InitializeComponent();

        Theme.Register(this);
    }

    public void ApplyTheme(Theme theme)
    {
        PAsyncPanel.ApplyColor(theme.Normal);
        OnApplyTheme(theme);
    }

    public void ChangeView(ViewEventArgs e) => OnChangeView(this, e);

    public void RunActionAsync(Action callback)
    {
        var loading = StartAsyncIndication();
        Queue.Enqueue(() => {
            Utility.Try(callback);
            loading.Dispose();
        }, Token);
    }

    public void RunAsyncTask(InvokeTask task)
    {
        var loading = StartAsyncIndication();
        Queue.Enqueue(async () => {
            await Utility.Try(() => task.Invoke(Token));
            loading.Dispose();
        }, Token);
    }

    public IDisposable StartAsyncIndication()
    {
        this.CrossThread(BeginAsyncIndication);
        return new Subscription(() => this.CrossThread(EndAsyncIndication));
    }

    protected override void Dispose(bool disposing)
    {
        Token.Dispose();
        base.Dispose(disposing);
    }

    protected virtual void OnApplyTheme(Theme theme)
    {
    }

    protected virtual void OnChangeView(object sender, ViewEventArgs e) => Controls.OfType<AnimatedPanel>()
          .FirstOrDefault()
          .CrossThread(p => p.ChangeView(e));

    protected override void OnLoad(EventArgs e)
    {
        if (DesignMode)
        {
            return; // prevent controls being added when in design mode
        }

        SuspendLayout();

        BarberPole.BackColor = Color.Transparent;
        BarberPole.Dock = DockStyle.Fill;
        BarberPole.StylePreset = StylePresets.Custom;
        BarberPole.SpokeThickness = 8;
        BarberPole.InnerCircleRadius = 30;
        BarberPole.OuterCircleRadius = 35;
        BarberPole.NumberSpoke = 100;
        BarberPole.Color = SystemColors.ControlDark;
        BarberPole.RotationSpeed = 35;

        PAsyncPanel.Dock = DockStyle.Fill;
        PAsyncPanel.Controls.Add(BarberPole);
        Controls.Add(PAsyncPanel);
        PAsyncPanel.BringToFront();

        ResumeLayout(false);
    }

    protected override void WndProc(ref Message m)
    {
        //
        // Filter out maximize/minimize/restore commands when IsAsyncBusy==TRUE.
        // Also filter out double-clicks of the titlebar to prevent the user min/max'ing that way.
        // Otherwise there will bad bad graphical glitches...
        if (IsAsyncBusy)
        {
            if (m.Msg == 0x112 /* WM_SYSCOMMAND */)
            {
                int w = m.WParam.ToInt32();
                if (w is 0xf120 /* SC_RESTORE */ or 0xf030 /* SC_MAXIMIZE */ or 0xf020 /* SC_MINIMIZE */)
                {
                    return; // short circuit
                }
            }
            else if (m.Msg == 0xa3 /* WM_NCLBUTTONDBLCLK */)
            {
                return; // short circuit
            }
        }

        base.WndProc(ref m);
    }

    private static void Grayscale(Bitmap b)
    {
        // GDI+ still lies to us - the return format is BGR, NOT RGB.
        var bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        var stride = bmData.Stride;
        var Scan0 = bmData.Scan0;

        unsafe
        {
            byte* p = (byte*)(void*)Scan0;
            int nOffset = stride - (3 * b.Width);

            for (int y = 0; y < b.Height; ++y)
            {
                for (int x = 0; x < b.Width; ++x)
                {
                    var blue = p[0];
                    var green = p[1];
                    var red = p[2];

                    p[0] = p[1] = p[2] = (byte)((.299 * red) + (.587 * green) + (.114 * blue));

                    p += 3;
                }
                p += nOffset;
            }
        }

        b.UnlockBits(bmData);
    }

    private void BeginAsyncIndication()
    {
        if (DesignMode)
        {
            return;
        }

        if (!IsAsyncBusy)
        {
            if (IsHandleCreated)
            {
                IntPtr srcDc = WindowsApi.GetDC(Handle);
                using var buffer = new GraphicsBuffer(ClientRectangle.Width, ClientRectangle.Height);
                var g = buffer.Graphics;

                // Copy image of form into bitmap...
                IntPtr bmpDc = g.GetHdc();
                WindowsApi.BitBlt(bmpDc, 0, 0, buffer.Width, buffer.Height, srcDc, 0, 0, 0x00CC0020 /* SRCCOPY */);

                _ = WindowsApi.ReleaseDC(Handle, srcDc);
                g.ReleaseHdc(bmpDc);

                Grayscale(buffer.Image);

                // Apply translucent overlay...
                g.FillRectangle(PAsyncPanel.Brush, 0, 0, buffer.Width, buffer.Height);

                // Store bitmap so that it can be painted by asyncPanel...
                PAsyncPanel.BackgroundImage = buffer.Image;
            }

            // Show asyncPanel...
            PAsyncPanel.Visible = BarberPole.Active = true;
        }

        Interlocked.Increment(ref ReferenceCount);
    }

    private void EndAsyncIndication()
    {
        if (IsAsyncBusy)
        {
            Interlocked.Decrement(ref ReferenceCount);
            if (ReferenceCount < 0)
            {
                ReferenceCount = 0;
            }
        }

        if (!IsAsyncBusy)
        {
            PAsyncPanel.Visible = BarberPole.Active = false;
            PAsyncPanel.BackgroundImage?.Dispose();
            PAsyncPanel.BackgroundImage = null;
        }
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        //
        // OneForm
        //
        BackColor = Color.White;
        ClientSize = new Size(286, 253);
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        Name = "OneForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "OneForm";
        ResumeLayout(false);
    }
}