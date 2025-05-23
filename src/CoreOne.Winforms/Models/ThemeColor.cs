namespace CoreOne.Winforms.Models;

public readonly struct ThemeColor(Color backColor, Color foreColor, Color? borderColor = null) : IEquatable<ThemeColor>
{
    public Color BackColor { get; } = backColor;
    public Color BorderColor { get; } = borderColor ?? backColor.Darken(0.15f);
    public Color ForeColor { get; } = foreColor;

    public static bool operator !=(ThemeColor left, ThemeColor right) => !(left == right);

    public static bool operator ==(ThemeColor left, ThemeColor right) => left.Equals(right);

    public override bool Equals(object? obj) => obj is ThemeColor color && color.BackColor == BackColor && color.ForeColor == ForeColor;

    public bool Equals(ThemeColor other) => other.BackColor == BackColor && other.ForeColor == ForeColor;

    public override int GetHashCode() => 27 * BackColor.GetHashCode() * ForeColor.GetHashCode();
}