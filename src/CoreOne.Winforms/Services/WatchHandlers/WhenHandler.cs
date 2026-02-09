using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Services.WatchHandlers;

public abstract class WhenHandler(PropertyGridItem gridItem, WhenAttribute[] attributes) : WatchHandler(gridItem.Property)
{
    protected WhenAttribute[] Attributes { get; } = attributes;
    protected Type ModelType { get; private set; } = Types.Void;
    protected PropertyGridItem PropertyGridItem { get; } = gridItem;
    protected Dictionary<string, Metadata> TargetProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

    protected abstract void OnCondiition(object model, IReadOnlyList<bool> flags);

    protected override void OnInitialize(object model)
    {
        ModelType = model.GetType();
        var properties = MetaType.GetMetadatas(ModelType).ToDictionary();
        Attributes.Select(p => p.PropertyName)
            .Each(p => {
                Dependencies.Add(p);
                TargetProperties[p] = properties.Get(p);
            });
    }

    protected override void OnRefresh(object model, bool isFirst)
    {
        PropertyGridItem.InputControl.CrossThread(() => {
            // All conditions must be satisfied (AND logic)
            var flags = new List<bool>(Attributes.Length);
            foreach (var attr in Attributes)
            {
                if (!TargetProperties.TryGetValue(attr.PropertyName, out var targetProperty))
                    continue;

                var targetValue = targetProperty.GetValue(model);
                flags.Add(targetValue.CompareToObject(attr.ExpectedValue, attr.ComparisonType));
            }
            OnCondiition(model, flags);
        });
    }
}