namespace CoreOne.Winforms.Models;

public enum GridColumnSpan
{
    None = 0,
    /// <summary>
    /// Span 1 column
    /// </summary>
    One = 1,
    /// <summary>
    /// Span 2 columns
    /// </summary>
    Two = 2,
    /// <summary>
    /// Span 3 columns
    /// </summary>
    Three = 3,
    /// <summary>
    /// Span 4 columns
    /// </summary>
    Four = 4,
    /// <summary>
    /// Span 5 columns
    /// </summary>
    Five = 5,
    /// <summary>
    /// Span 6 columns
    /// </summary>
    Six = 6,
    /// <summary>
    /// Default span (6 columns)
    /// </summary>
    Default = Six,
    /// <summary>
    /// Helf width (3 columns)
    /// </summary>
    Half = Three,
    /// <summary>
    /// Full width (6 columns)
    /// </summary>
    Full = Six
}