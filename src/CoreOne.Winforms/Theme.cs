using CoreOne.Drawing;
using CoreOne.Reactive;
using CoreOne.Winforms.Models;
using System.Collections.ObjectModel;

namespace CoreOne.Winforms;

public partial class Theme
{
    private static readonly Color DarkColor = ColorTranslator.FromHtml("#252525");
    private static readonly Color LightColor = Color.White;
    private static readonly BehaviorSubject<Theme> Subject;
    private Lazy<ReadOnlyCollection<ThemeColor>> _Swatch;
    public static Theme Current => Subject.Value!;

    public static Theme Dark => new() {
        Normal = new ThemeColor(DarkColor, LightColor, DarkColor.Lighten(0.2f)),
        Danger = new ThemeColor(Color.Firebrick, Color.White),
        Primary = new ThemeColor(Color.DodgerBlue, Color.White),
        Success = new ThemeColor(Color.DarkGreen, Color.White),
    };

    public static Theme Light => new() {
        Normal = new ThemeColor(LightColor, DarkColor),
        Danger = new ThemeColor(LightColor, Color.Firebrick),
        Primary = new ThemeColor(Color.DodgerBlue, Color.White),
        Success = new ThemeColor(Color.DarkGreen, Color.White),
    };

    public ThemeColor Danger { get; set; }
    public bool IsDark => Normal.BackColor.GetBrightness() < 0.48f;
    public ThemeColor Normal { get; set; }
    public ThemeColor Primary { get; set; }
    public ThemeColor Success { get; set; }
    public ReadOnlyCollection<ThemeColor> Swatch => _Swatch.Value;
    public ThemeColor Warning { get; set; }

    static Theme() => Subject = new BehaviorSubject<Theme>(Dark);

    public Theme()
    {
        Normal = new ThemeColor(ColorTranslator.FromHtml("#383838"), Color.White, ColorTranslator.FromHtml("#222"));
        Primary = new ThemeColor(ColorTranslator.FromHtml("#0869cf"), Color.White);
        Success = new ThemeColor(Color.DarkGreen, Color.White);
        Danger = new ThemeColor(Color.Firebrick, Color.White);
        Warning = new ThemeColor(ColorTranslator.FromHtml("#E99D49"), Color.White);
        _Swatch = new Lazy<ReadOnlyCollection<ThemeColor>>(() => {
            var one = Normal.BackColor;
            var other = ColorExtensions.DarkenOnLightLerp(one, 0.95f);
            var gradients = Drawings.GetGradients(one, other, 5)
                .Select(c => new ThemeColor(c, c.DarkenOnLightLerp(0.95f)));
            return new ReadOnlyCollection<ThemeColor>([.. gradients]);
        });
    }

    public static void Publish(Theme value)
    {
        value._Swatch = new Lazy<ReadOnlyCollection<ThemeColor>>(() => {
            var one = value.Normal.BackColor;
            var other = ColorExtensions.DarkenOnLightLerp(one, 0.95f);
            var gradients = Drawings.GetGradients(one, other, 5)
                .Select(c => new ThemeColor(c, c.DarkenOnLightLerp(0.95f)));
            return new ReadOnlyCollection<ThemeColor>([.. gradients]);
        });
        Subject.OnNext(value);
    }

    public static void ReapplyTheme(Control control, Theme? theme = null)
    {
        var type = control is IControlTheme ctheme ? ctheme.ThemeType : ThemeType.Default;
        theme ??= Current;
        if (control is IControlBorder cborder)
        {
            cborder.BorderColor = theme.BorderColor(type);
        }

        control.ApplyColor(theme.AsColor(type));
        if (control is ITheme itheme)
        {
            itheme.ApplyTheme(theme);
        }

        control.Invalidate();
    }

    public static void Register(Control? control, Action<Theme>? onThemeChanged = null)
    {
        if (control is null)
            return;

        Subject.Subscribe(next => {
            ReapplyTheme(control, next);
            onThemeChanged?.Invoke(next);
        }, control.CreateSToken());
    }

    public ThemeColor AsAccent(ThemeType theme = ThemeType.Default)
    {
        var current = this;
        switch (theme)
        {
            case ThemeType.Success:
                return new ThemeColor(current.Normal.BackColor, current.Success.ForeColor, current.Success.BorderColor);

            case ThemeType.Warning:
                return new ThemeColor(current.Normal.BackColor, current.Warning.ForeColor, current.Warning.BorderColor);

            case ThemeType.Danger:
                return new ThemeColor(current.Normal.BackColor, current.Danger.ForeColor, current.Danger.BorderColor);

            case ThemeType.Primary:
                return new ThemeColor(current.Normal.BackColor, current.Primary.ForeColor, current.Primary.BorderColor);

            default:
                var copy = current.Normal;
                return new ThemeColor(copy.BackColor.DarkenOnDarkLerp(0.1f), copy.ForeColor, copy.BorderColor);
        }
    }

    public ThemeColor AsColor(ThemeType theme) => theme switch {
        ThemeType.Success => Success,
        ThemeType.Warning => Warning,
        ThemeType.Danger => Danger,
        ThemeType.Primary => Primary,
        _ => Normal
    };

    public Color BackColor(ThemeType theme = ThemeType.Default, int? alpha = null)
    {
        var current = this;
        return theme switch {
            ThemeType.Success => alpha.HasValue ? Color.FromArgb(alpha.Value, current.Success.BackColor) : current.Success.BackColor,
            ThemeType.Warning => alpha.HasValue ? Color.FromArgb(alpha.Value, current.Warning.BackColor) : current.Warning.BackColor,
            ThemeType.Danger => alpha.HasValue ? Color.FromArgb(alpha.Value, current.Danger.BackColor) : current.Danger.BackColor,
            ThemeType.Primary => alpha.HasValue ? Color.FromArgb(alpha.Value, current.Primary.BackColor) : current.Primary.BackColor,
            _ => alpha.HasValue ? Color.FromArgb(alpha.Value, current.Normal.BackColor) : current.Normal.BackColor,
        };
    }

    public Color BorderColor(ThemeType theme = ThemeType.Default)
    {
        var current = this;
        return theme switch {
            ThemeType.Success => current.Success.BorderColor,
            ThemeType.Warning => current.Warning.BorderColor,
            ThemeType.Danger => current.Danger.BorderColor,
            ThemeType.Primary => current.Primary.BorderColor,
            _ => current.Normal.BorderColor,
        };
    }

    public Color ForeColor(ThemeType theme = ThemeType.Default, int? alpha = null)
    {
        var current = this;
        return theme switch {
            ThemeType.Success => alpha.HasValue ? Color.FromArgb(alpha.Value, current.Success.ForeColor) : current.Success.ForeColor,
            ThemeType.Warning => alpha.HasValue ? Color.FromArgb(alpha.Value, current.Warning.ForeColor) : current.Warning.ForeColor,
            ThemeType.Danger => alpha.HasValue ? Color.FromArgb(alpha.Value, current.Danger.ForeColor) : current.Danger.ForeColor,
            ThemeType.Primary => alpha.HasValue ? Color.FromArgb(alpha.Value, current.Primary.ForeColor) : Primary.ForeColor,
            _ => alpha.HasValue ? Color.FromArgb(alpha.Value, current.Normal.ForeColor) : current.Normal.ForeColor,
        };
    }
}