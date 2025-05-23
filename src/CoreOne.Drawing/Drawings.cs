using CoreOne.Extensions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CoreOne.Drawing;

public static class Drawings
{
    public static readonly Color BackColor = ColorTranslator.FromHtml("#dadcdf");
    public static readonly Color DarkBackColor = ColorTranslator.FromHtml("#90949a");
    public static readonly Color LightBackColor = ColorTranslator.FromHtml("#F5F5F5");
    private static readonly byte[] _Pngiconheader = [0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    public static Color BlendColor(Color backgroundColor, Color frontColor)
    {
        var alpha = frontColor.A / 255f;
        var invAlpha = 1f - alpha;
        byte r = (byte)((frontColor.R * alpha) + (backgroundColor.R * invAlpha));
        byte g = (byte)((frontColor.G * alpha) + (backgroundColor.G * invAlpha));
        byte b = (byte)((frontColor.B * alpha) + (backgroundColor.B * invAlpha));
        byte a = (byte)(Math.Min(255, frontColor.A + (backgroundColor.A * invAlpha)));

        return Color.FromArgb(a, r, g, b);
    }

    public static Icon DrawIcon(Image img, int size = 16)
    {
        Icon? ico = null;
        using (var bmp = new Bitmap(img, new Size(size, size)))
        {
            byte[]? png = null;
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                png = ms.ToArray();
            }
            using (var ms = new MemoryStream())
            {
                size = size.Bounds(1, 255);
                _Pngiconheader[6] = (byte)size;
                _Pngiconheader[7] = (byte)size;
                _Pngiconheader[14] = (byte)(png.Length & 255);
                _Pngiconheader[15] = (byte)(png.Length / 256);
                _Pngiconheader[18] = (byte)_Pngiconheader.Length;

                ms.Write(_Pngiconheader, 0, _Pngiconheader.Length);
                ms.Write(png, 0, png.Length);
                ms.Position = 0;
                ico = new Icon(ms);
            }
        }
        return ico;
    }

    public static Bitmap? DrawText(string text, Font font, StringFormat strformat, Color c1, Color? c2 = null, Color? shadow = null)
    {
        Bitmap? img = null;
        if (!string.IsNullOrEmpty(text))
        {
            SizeF sz;
            Bitmap bmp;
            using (bmp = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bmp))
                sz = g.MeasureString(text, font);
            bmp = new Bitmap((int)sz.Width, (int)sz.Height);
            var viewport = new Rectangle(0, 0, (int)sz.Width, (int)sz.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Pretty();
                RenderText(g, text, font, strformat, viewport, c1, c2, shadow);
                g.Flush();
            }
            img = bmp;
        }
        return img;
    }

    public static Brush GetBrush(Rectangle viewport, Color light, Color? dark, float angle = 90F)
    {
        Func<Brush>
            sb = () => new SolidBrush(light),
            lg = () => new LinearGradientBrush(viewport, light, dark.GetValueOrDefault(Color.Transparent), angle);
        return dark.HasValue ? lg() : sb();
    }

    public static StringFormat GetContentAlignment(ContentAlignment alignment, bool autoElipsis = false)
    {
        var sf = new StringFormat();
        switch (alignment)
        {
            case ContentAlignment.TopLeft:
            case ContentAlignment.TopCenter:
            case ContentAlignment.TopRight:
                sf.LineAlignment = StringAlignment.Near;
                break;

            case ContentAlignment.MiddleLeft:
            case ContentAlignment.MiddleCenter:
            case ContentAlignment.MiddleRight:
                sf.LineAlignment = StringAlignment.Center;
                break;

            case ContentAlignment.BottomLeft:
            case ContentAlignment.BottomCenter:
            case ContentAlignment.BottomRight:
                sf.LineAlignment = StringAlignment.Far;
                break;
        }
        switch (alignment)
        {
            case ContentAlignment.TopLeft:
            case ContentAlignment.MiddleLeft:
            case ContentAlignment.BottomLeft:
                sf.Alignment = StringAlignment.Near;
                break;

            case ContentAlignment.TopCenter:
            case ContentAlignment.MiddleCenter:
            case ContentAlignment.BottomCenter:
                sf.Alignment = StringAlignment.Center;
                break;

            case ContentAlignment.TopRight:
            case ContentAlignment.MiddleRight:
            case ContentAlignment.BottomRight:
                sf.Alignment = StringAlignment.Far;
                break;
        }

        if (autoElipsis)
        {
            sf.Trimming = StringTrimming.EllipsisCharacter;
            sf.FormatFlags |= StringFormatFlags.LineLimit;
        }
        return sf;
    }

    public static IEnumerable<Color> GetGradients(Color start, Color end, int steps)
    {
        var stepA = (end.A - start.A) / (steps - 1);
        var stepR = (end.R - start.R) / (steps - 1);
        var stepG = (end.G - start.G) / (steps - 1);
        var stepB = (end.B - start.B) / (steps - 1);

        for (var i = 0; i < steps; i++)
        {
            yield return Color.FromArgb(start.A + (stepA * i),
                                        start.R + (stepR * i),
                                        start.G + (stepG * i),
                                        start.B + (stepB * i));
        }
    }

    public static void GrayScale(Bitmap b)
    {
        using var g = Graphics.FromImage(b);
        using var ia = new ImageAttributes();
        var cm = new ColorMatrix([[0.3f, 0.3f, 0.3f, 0, 0], [0.59f, 0.59f, 0.59f, 0, 0], [0.11f, 0.11f, 0.11f, 0, 0], [0, 0, 0, 1, 0, 0], [0, 0, 0, 0, 1, 0], [0, 0, 0, 0, 0, 1]]);
        ia.SetColorMatrix(cm);
        g.DrawImage(b, new Rectangle(0, 0, b.Width, b.Height), 0, 0, b.Width, b.Height, GraphicsUnit.Pixel, ia);
    }

    public static void RenderText(Graphics graphics, string text, Font font, StringFormat strformat, Rectangle viewport, Color c1, Color? c2 = null, Color? shadow = null)
    {
        using var path = new GraphicsPath();
        if (shadow.HasValue)
        {
            Rectangle viewshadow = viewport;
            viewshadow.Offset(-1, 1);
            using var spath = new GraphicsPath();
            spath.AddString(text, font.FontFamily, (int)font.Style, font.Size + 0.5f, viewshadow, strformat);
            graphics.FillPath(new SolidBrush(shadow.Value), spath);
        }
        path.AddString(text, font.FontFamily, (int)font.Style, font.Size, viewport, strformat);
        using Brush brush = GetBrush(viewport, c1, c2, -90);
        graphics.FillPath(brush, path);
    }

    public static Bitmap ResizeScaled(Bitmap img, int maxsize, bool usewidth)
    {
        double
            rx = img.Width / (double)maxsize,
            ry = img.Height / (double)maxsize,
            ratio = usewidth ? rx : ry;
        int
            w = Convert.ToInt32(img.Width / ratio),
            h = Convert.ToInt32(img.Height / ratio);
        var bmp = new Bitmap(w, h);
        using (var g = Graphics.FromImage(bmp))
            g.DrawImage(img, 0, 0, w, h);
        return bmp;
    }

    public static GraphicsPath RoundRect(float x, float y, float width, float height, float radius)
    {
        GraphicsPath gp = new GraphicsPath();
        gp.AddLine(x + radius, y, x + width - (radius * 2), y);
        gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);

        gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
        gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90);

        gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
        gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);

        gp.AddLine(x, y + height - (radius * 2), x, y + radius);
        gp.AddArc(x, y, radius * 2, radius * 2, 180, 90);

        gp.CloseFigure();
        return gp;
    }

    public static GraphicsPath RoundRect(RectangleF r, float r1, float r2, float r3, float r4)
    {
        float x = r.X, y = r.Y, w = r.Width, h = r.Height;
        var rr = new GraphicsPath();
        rr.AddBezier(x, y + r1, x, y, x + r1, y, x + r1, y);
        rr.AddLine(x + r1, y, x + w - r2, y);
        rr.AddBezier(x + w - r2, y, x + w, y, x + w, y + r2, x + w, y + r2);
        rr.AddLine(x + w, y + r2, x + w, y + h - r3);
        rr.AddBezier(x + w, y + h - r3, x + w, y + h, x + w - r3, y + h, x + w - r3, y + h);
        rr.AddLine(x + w - r3, y + h, x + r4, y + h);
        rr.AddBezier(x + r4, y + h, x, y + h, x, y + h - r4, x, y + h - r4);
        rr.AddLine(x, y + h - r4, x, y + r1);
        return rr;
    }

    public static GraphicsPath RoundRect(RectangleF rect, float diameter)
    {
        var path = new GraphicsPath();
        var arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
        path.AddArc(arc, 180, 90);
        arc.X = rect.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = rect.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}