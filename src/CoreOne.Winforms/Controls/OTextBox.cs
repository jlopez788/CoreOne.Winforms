using CoreOne.Models;
using CoreOne.Winforms.Native;
using CoreOne.Winforms.Services;
using System.ComponentModel;

namespace CoreOne.Winforms.Controls;

public class OTextBox : TextBox, IControlBorder
{
    private readonly BackingField<string> BakPlaceholder;
    private readonly ControlStateManager StateManager;
    private readonly SToken Token;

    [RefreshProperties(RefreshProperties.Repaint)]
    public Color BorderColor { get; set; }

    [RefreshProperties(RefreshProperties.Repaint)]
    public string? Placeholder {
        get { return BakPlaceholder.Value; }
        set { BakPlaceholder.UpdateValue(value); }
    }

    public OTextBox()
    {
        Token = this.CreateSToken();
        BorderStyle = BorderStyle.FixedSingle;
        BakPlaceholder = new BackingField<string>();
        BakPlaceholder.SubscribeOnValueChanged(p => {
            Invalidate();
            BakPlaceholder.MarkResolved();
        }, Token);
        Token.Register(BakPlaceholder);
        StateManager = new ControlStateManager(this, false);
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == WindowsApi.WM_PAINT)
        {
            if (!StateManager.IsFocused && string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Placeholder))
            {
                using var g = CreateGraphics();
                var border = StateManager.BorderColor();
                var view = ClientRectangle;
                view.Inflate(-1, -1);
                TextRenderer.DrawText(g, Placeholder, Font, view, Color.Gray, BackColor, TextFormatFlags.Top | TextFormatFlags.Left);
            }
        }
    }
}