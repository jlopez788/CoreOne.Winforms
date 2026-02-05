using CoreOne.Attributes;
using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Services.WatchHandlers;

/// <summary>
/// Watch handler that conditionally enables or disables controls based on property values.
/// Supports complex conditional logic with comparison operators (EqualTo, NotEqualTo, GreaterThan, etc.).
/// </summary>
/// <remarks>
/// <para>Multiple [EnabledWhen] attributes on the same property are combined with AND logic.</para>
/// <para>The control is enabled only when ALL conditions are satisfied.</para>
/// </remarks>
/// <example>
/// <code>
/// // Enable only when IsActive is true
/// [EnabledWhen(nameof(IsActive), true)]
/// public string Notes { get; set; }
///
/// // Enable only when Age is greater than or equal to 18
/// [EnabledWhen(nameof(Age), 18, ComparisonType.GreaterThanOrEqualTo)]
/// public bool CanVote { get; set; }
///
/// // Multiple conditions (both must be true)
/// [EnabledWhen(nameof(IsActive), true)]
/// [EnabledWhen(nameof(Role), "Admin")]
/// public string AdminNotes { get; set; }
/// </code>
/// </example>
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

        protected override void OnRefresh(object model, bool isFirst)
        {
            PropertyGridItem.InputControl.CrossThread(() => {
                // All conditions must be satisfied (AND logic)
                var isEnabled = true;
                foreach (var attr in EnabledAttributes)
                {
                    if (!TargetProperties.TryGetValue(attr.PropertyName, out var targetProperty))
                        continue;

                    var targetValue = targetProperty.GetValue(model);
                    var conditionMet = EvaluateCondition(targetValue, attr.ExpectedValue, attr.ComparisonType);

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
                var typeName = sourceValue.GetType().Name;
                throw new ArgumentException(
                    $"Source value of type '{typeName}' does not implement IComparable interface. " +
                    $"Comparison operations require IComparable. Consider using ComparisonType.EqualTo/NotEqualTo " +
                    $"with types that implement IEquatable<T>, or implement IComparable on '{typeName}'.");
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
                _ => throw new InvalidOperationException(
                    $"Unsupported comparison type '{comparisonType}'. " +
                    $"Supported types: EqualTo, NotEqualTo, GreaterThan, LessThan, GreaterThanOrEqualTo, LessThanOrEqualTo."),
            };
        }
    }

    protected override IWatchHandler? OnCreateInstance(PropertyGridItem gridItem, EnabledWhenAttribute[] attributes)
    {
        return new EnabledWhenHandlerInstance(gridItem, attributes);
    }
}