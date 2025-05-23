using CoreOne.Extensions;
using System.Drawing;

namespace CoreOne.Drawing.Extensions;

public static class ColorExtensions
{
    public static Color Blend(this Color color, Color backColor, float amount)
    {
        byte r = (byte)((color.R * amount) + (backColor.R * (1 - amount)));
        byte g = (byte)((color.G * amount) + (backColor.G * (1 - amount)));
        byte b = (byte)((color.B * amount) + (backColor.B * (1 - amount)));
        return Color.FromArgb(r, g, b);
    }

    public static Color ContrastColor(this Color color)
    {
        var d = color.IsDark() ? 250 : 44;
        return Color.FromArgb(color.A, d, d, d);
    }

    public static Color Darken(this Color color, float p) => color.Lerp(Color.Black, p);

    public static Color DarkenOnDarkLerp(this Color color, float p)
    {
        var isDark = color.IsDark();
        return isDark ? color.Darken(p) : color.Lighten(p);
    }

    public static Color DarkenOnLightLerp(this Color color, float p)
    {
        var isDark = color.IsDark();
        return isDark ? color.Lighten(p) : color.Darken(p);
    }

    public static bool IsDark(this Color color) => color.GetBrightness() < 0.48;

    public static Color Lerp(this Color colorstart, Color colorend, float percentage)
    {
        int[]
            cs = colorstart.ToArray(true),
            ce = colorend.ToArray(true),
            cf = new int[4];

        for (var i = 0; i < cs.Length; i++)
            cf[i] = Convert.ToInt32(cs[i].Lerp(ce[i], percentage)).Bounds(0, 255);

        return Color.FromArgb(cf[0], cf[1], cf[2], cf[3]);
    }

    public static Color Lighten(this Color color, float p) => color.Lerp(Color.White, p);

    public static Color SetAlpha(this Color color, int alpha) => Color.FromArgb(alpha.Bounds(0, 255), color);

    public static int[] ToArray(this Color color, bool includealpha = false)
    {
        var list = new List<int>(4);
        if (includealpha)
            list.Add(color.A);
        list.Add(color.R);
        list.Add(color.G);
        list.Add(color.B);
        return [.. list];
    }

    public static Color ToGray(this Color c, bool includealpha = false)
    {
        var avg = (int)c.ToArray(includealpha)
                         .Average();
        return Color.FromArgb(avg, avg, avg);
    }
}