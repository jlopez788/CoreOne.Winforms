namespace CoreOne.Winforms.Attributes;

/// <summary>
/// Specifies that a numeric property should be displayed as a star rating control
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class RatingAttribute : Attribute
{
    /// <summary>
    /// Gets the maximum rating value (default is 5)
    /// </summary>
    public int MaxRating { get; }

    /// <summary>
    /// Initializes a new instance of the RatingAttribute with default max rating of 5
    /// </summary>
    public RatingAttribute() : this(5)
    {
    }

    /// <summary>
    /// Initializes a new instance of the RatingAttribute
    /// </summary>
    /// <param name="maxRating">The maximum rating value</param>
    public RatingAttribute(int maxRating)
    {
        if (maxRating <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRating), "MaxRating must be greater than 0");

        MaxRating = maxRating;
    }
}