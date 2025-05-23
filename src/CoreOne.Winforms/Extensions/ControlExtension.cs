using CoreOne.Reflection;
using CoreOne.Winforms.Models;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CoreOne.Winforms.Extensions;

public static class ControlExtension
{
    public static TControl AddControls<TControl>(this TControl control, IEnumerable<Control>? controls, bool resumeLayout = false) where TControl : Control
    {
        var children = controls?.ToArray();
        if (children?.Length > 0)
        {
            control.SuspendLayout();
            control.Controls.AddRange(children);
            control.ResumeLayout(resumeLayout);
        }
        return control;
    }

    public static void ApplyColor(this Control control, ThemeColor color)
    {
        control.BackColor = color.BackColor;
        control.ForeColor = color.ForeColor;
    }

    [return: NotNullIfNotNull(nameof(parent))]
    public static TControl? ClearControls<TControl>(this TControl? parent) where TControl : Control
    {
        if (parent != null)
        {
            parent.SuspendLayout();
            foreach (Control control in parent.Controls)
            {
                control.Dispose();
            }
            parent.Controls.Clear();
            parent.ResumeLayout(true);
        }
        return parent;
    }

    public static IResult CrossThread<TControl>(this TControl? control, Action callback) where TControl : ISynchronizeInvoke
    {
        return Utility.Try(() => {
            if (control?.InvokeRequired == true)
            {
                control.Invoke(() => callback(), null);
            }
            else
            {
                callback();
            }
        });
    }

    public static IResult CrossThread<TControl>(this TControl? control, Action<TControl> callback) where TControl : ISynchronizeInvoke
    {
        return Utility.Try(() => {
            if (control is not null)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(() => callback(control), null);
                }
                else
                {
                    callback(control);
                }
            }
        });
    }

    public static IResult<R> CrossThread<TControl, R>(this TControl? t, Func<R> callback) where TControl : ISynchronizeInvoke
    {
        return Utility.Try(() => t?.InvokeRequired == true ? (R)t.Invoke(callback, null)! : callback.Invoke());
    }

    public static void CustomRenderControl<TControl>(this TControl control, ControlStyles? styles = null) where TControl : Control
    {
        var method = MetaType.GetInvokeMethod(typeof(TControl), "SetStyle", [typeof(ControlStyles), Types.Bool]);
        styles = styles.GetValueOrDefault(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint);
        method.Invoke(control, [styles.Value, true]);
    }

    public static int GetOffsetX(this Control control, int gap = 0) => control.Location.X + control.Width + gap;

    public static int GetOffsetY(this Control control, int gap = 0) => control.Location.Y + control.Height + gap;

    public static void OnEnterClick<TControl, TButton>(this TControl control, TButton button) where TControl : Control where TButton : IButtonControl => control.OnEnterDo(button.PerformClick);

    public static void OnEnterDo<TControl>(this TControl control, Action onEnter) where TControl : Control
    {
        if (control != null && onEnter != null)
        {
            control.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    onEnter.Invoke();
                }
            };
        }
    }

    public static void SetControls<TControl>(this TControl? parent, IEnumerable<Control>? controls, bool resumeLayout = false) where TControl : Control
    {
        if (parent is null)
            return;

        parent.SuspendLayout();
        foreach (Control control in parent.Controls)
            control.Dispose();
        parent.Controls.Clear();
        var children = controls?.ToArray();
        if (children?.Length > 0)
            parent.Controls.AddRange(children);
        parent.ResumeLayout(resumeLayout);
    }

    public static bool ShowDialogExt(this Form form, IWin32Window? parent = null, DialogResult result = DialogResult.OK) => form.ShowDialog(parent) == result;
}