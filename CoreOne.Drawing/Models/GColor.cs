using System.Drawing;

namespace CoreOne.Drawing.Models;

public class GColor(Color start, Color? end = null)
{
    public static readonly GColor Empty = new(Color.Transparent, Color.Transparent);
    public Color? End { get; set; } = end;
    public Color Start { get; set; } = start;

    public static implicit operator GColor(Color color) => new(color);

    public GColor Darken(float start, float? end = null)
    {
        var endColor = End?.Darken(end.GetValueOrDefault(start));
        return new GColor(Start.Darken(start), endColor);
    }

    public Brush GetBrush(Rectangle view, float angle = 90f) => Drawings.GetBrush(view, Start, End, angle);

    public GColor Lighten(float start, float? end = null)
    {
        var endColor = End?.Lighten(end.GetValueOrDefault(start));
        return new GColor(Start.Lighten(start), endColor);
    }
}