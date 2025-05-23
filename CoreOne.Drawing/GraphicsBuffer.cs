using System.Drawing;

namespace CoreOne.Drawing;

public class GraphicsBuffer : IDisposable
{
    public Graphics Graphics { get; }
    public Bitmap Image { get; }
    public int Width { get; init; }
    public int Height { get; init; }

    public GraphicsBuffer(int width, int height)
    {
        Width = width;
        Height = height;
        Image = new Bitmap(width, height);
        Graphics = Graphics.FromImage(Image);
    }

    public GraphicsBuffer Complete(Graphics gfx)
    {
        gfx.DrawImage(Image, 0, 0);
        return this;
    }

    public void Dispose()
    {
        Image.Dispose();
        Graphics.Dispose();

        GC.SuppressFinalize(this);
    }

    public GraphicsBuffer Fill(Brush brush, Rectangle? viewport = null)
    {
        Graphics.FillRectangle(brush, viewport.GetValueOrDefault(GetViewport()));
        return this;
    }

    public Rectangle GetViewport() => new(0, 0, Image.Width, Image.Height);

    public GraphicsBuffer Pretty()
    {
        Graphics.Pretty();
        return this;
    }
}