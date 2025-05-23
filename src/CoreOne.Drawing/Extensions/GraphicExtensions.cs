using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace CoreOne.Drawing.Extensions;

public static class GraphicExtensions
{
    [return: NotNullIfNotNull(nameof(gfx))]
    public static Graphics? FillRectangle(this Graphics? gfx, Rectangle viewport, Color light, Color? dark = null, float angle = 90F)
    {
        if (gfx is not null)
        {
            using var brush = Drawings.GetBrush(viewport, light, dark, angle);
            gfx.FillRectangle(brush, viewport);
        }
        return gfx;
    }

    [return: NotNullIfNotNull(nameof(gfx))]
    public static Graphics? Pretty(this Graphics? gfx)
    {
        if (gfx is not null)
        {
            gfx.SmoothingMode = SmoothingMode.AntiAlias;
            gfx.CompositingMode = CompositingMode.SourceOver;
            gfx.CompositingQuality = CompositingQuality.HighQuality;
            gfx.InterpolationMode = InterpolationMode.High;
            gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }
        return gfx;
    }
}