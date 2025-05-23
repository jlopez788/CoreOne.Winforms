namespace CoreOne.Winforms;

/// <summary>
/// String Alignment
/// </summary>
public static class StringAlign
{
    /// <summary>
    /// Top Left
    /// </summary>
    public static StringFormat TopLeft => new() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

    /// <summary>
    /// Top Center
    /// </summary>
    public static StringFormat TopCenter => new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };

    /// <summary>
    /// Top Right
    /// </summary>
    public static StringFormat TopRight => new() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };

    /// <summary>
    /// Left
    /// </summary>
    public static StringFormat Left => new() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

    /// <summary>
    /// Center
    /// </summary>
    public static StringFormat Center => new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

    /// <summary>
    /// Right
    /// </summary>
    public static StringFormat Right => new() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };

    /// <summary>
    /// Bottom Left
    /// </summary>
    public static StringFormat BottomLeft => new() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Far };

    /// <summary>
    /// Bottom Center
    /// </summary>
    public static StringFormat BottomCenter => new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };

    /// <summary>
    /// Bottom Right
    /// </summary>
    public static StringFormat BottomRight => new() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Far };
}