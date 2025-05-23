using CoreOne.Drawing;
using CoreOne.Winforms.Services;
using System.ComponentModel;

namespace CoreOne.Winforms.Controls;

public class OLabel : Control
{
    private readonly ControlStateManager StateManager;
    private bool _IsLink;
    public bool AutoElipsis { get; set; }

    public override Font Font {
        get => base.Font;
        set => base.Font = new Font(value!.FontFamily, value!.Size, _IsLink ? FontStyle.Underline : FontStyle.Regular);
    }
    public bool IsLink {
        get => _IsLink;
        set {
            _IsLink = value;
            Font = new Font(Font!.FontFamily, Font.Size, value ? FontStyle.Underline : FontStyle.Regular);
        }
    }

    [DefaultValue(ContentAlignment.MiddleCenter)]
    public virtual ContentAlignment TextAlign { get; set; } = ContentAlignment.MiddleCenter;

    public OLabel()
    {
        StateManager = new ControlStateManager(this);
        this.CustomRenderControl();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        var alignment = Drawings.GetContentAlignment(TextAlign, AutoElipsis);
        var foreColor = StateManager.ForeColor();
        if (IsLink)
        {
            var borderColor = StateManager.ForeColor();
            foreColor = StateManager.State != State.Normal ? borderColor.DarkenOnLightLerp(0.2f) : foreColor;
        }

        graphics.Pretty();
        graphics.DrawString(Text, Font!, new SolidBrush(foreColor), DisplayRectangle, alignment);
    }
}