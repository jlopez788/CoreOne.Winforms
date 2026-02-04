using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Controls;

namespace CoreOne.Winforms.Services.ControlFactories;

/// <summary>
/// Factory for creating rating controls for numeric properties with [Rating] attribute
/// </summary>
public class RatingControlFactory : IControlFactory
{
    /// <summary>
    /// High priority (100) to ensure attribute-based factories take precedence over generic type-based factories
    /// </summary>
    public int Priority => 100;

    public bool CanHandle(Metadata property)
    {
        // Check if property has Rating attribute and is numeric
        var ratingAttr = property.GetCustomAttribute<RatingAttribute>();
        return ratingAttr != null && Types.IsNumberType(property.FPType);
    }

    public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        // Check if property has Rating attribute
        var ratingAttr = property.GetCustomAttribute<RatingAttribute>();
        if (ratingAttr == null || !Types.IsNumberType(property.FPType))
            return null;

        var ratingControl = new RatingControl {
            MaxRating = ratingAttr.MaxRating,
            Height = 30,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };

        return new(ratingControl,
            value => ratingControl.Value = Types.TryParse<int>(value?.ToString(), out var rating) ? rating : 0,
            () => ratingControl.ValueChanged += (s, e) => onValueChanged(ratingControl.Value));
    }
}