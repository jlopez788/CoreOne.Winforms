using CoreOne.Attributes;
using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Services.WatchHandlers;

/// <summary>
/// Handler for EnabledWhen attributes that controls control enabled state based on property values
/// </summary>
public class EnabledWhenHandler : WatchFactoryFromAttribute<EnabledWhenAttribute>, IWatchFactory
{
    private class EnabledWhenHandlerInstance(PropertyGridItem gridItem, EnabledWhenAttribute[] attributes) : WatchHandler(gridItem.Property)
    {
        private readonly EnabledWhenAttribute[] EnabledAttributes = attributes;
        private readonly PropertyGridItem PropertyGridItem = gridItem;
        private readonly Dictionary<string, Metadata> TargetProperties = new(StringComparer.OrdinalIgnoreCase);

        protected override void OnInitialize(object model)
        {
            var modelType = model.GetType();
            var properties = MetaType.GetMetadatas(modelType).ToDictionary();
            EnabledAttributes.SelectMany(p => p.PropertyNames)
                .Each(p => TargetProperties[p] = properties.Get(p));
        }

        protected override void OnRefresh(object model)
        {
            PropertyGridItem.InputControl.CrossThread(() => {
                var value = Property.GetValue(model);

                // All conditions must be satisfied (AND logic)
                var isEnabled = true;
                foreach (var attr in EnabledAttributes)
                {
                    if (!TargetProperties.TryGetValue(attr.PropertyName, out var targetProperty))
                        continue;

                    var targetValue = targetProperty.GetValue(model);
                    var conditionMet = EvaluateCondition(value, targetValue, attr.ComparisonType);

                    if (!conditionMet)
                    {
                        isEnabled = false;
                        break;
                    }
                }

                PropertyGridItem.InputControl.Enabled = isEnabled;
            });
        }

        private static bool EvaluateCondition(object? sourceValue, object? targetValue, ComparisonType comparisonType)
        {
            var sourceComparable = sourceValue as IComparable;
            var isSVNull = sourceComparable == null;
            var isTVNull = targetValue == null;

            if (sourceValue is not null && !typeof(IComparable).IsAssignableFrom(sourceValue.GetType()))
            {
                throw new ArgumentException($"Source value of type {sourceValue.GetType()} has not implemented IComparable interface");
            }

            return comparisonType switch {
                ComparisonType.LessThan => !isSVNull && !isTVNull && sourceComparable?.CompareTo(targetValue) < 0,
                ComparisonType.LessThanOrEqualTo => !isSVNull && !isTVNull && sourceComparable?.CompareTo(targetValue) <= 0,
                ComparisonType.NotEqualTo => (!isSVNull && isTVNull) || (isSVNull && !isTVNull) ||
                                             (!isSVNull && !isTVNull && sourceComparable?.CompareTo(targetValue) != 0),
                ComparisonType.EqualTo => (isSVNull && isTVNull) ||
                                          (!isSVNull && !isTVNull && sourceComparable?.CompareTo(targetValue) == 0),
                ComparisonType.GreaterThan => !isSVNull && !isTVNull && sourceComparable?.CompareTo(targetValue) > 0,
                ComparisonType.GreaterThanOrEqualTo => !isSVNull && !isTVNull && sourceComparable?.CompareTo(targetValue) >= 0,
                _ => throw new InvalidOperationException($"Unsupported comparison type: {comparisonType}"),
            };
        }
    }

    protected override IWatchHandler? OnCreateInstance(PropertyGridItem gridItem, EnabledWhenAttribute[] attributes)
    {
        return new EnabledWhenHandlerInstance(gridItem, attributes);
    }
}