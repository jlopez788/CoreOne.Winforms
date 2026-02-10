using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Services.WatchHandlers;

public class ClearWhenHandler : WatchFactoryFromAttribute<ClearWhenAttribute>, IWatchFactory
{
    private class ClearWhenHandlerInstance(PropertyGridItem gridItem, ClearWhenAttribute[] attributes) : WhenHandler(gridItem, attributes)
    {
        protected Lazy<object?> LazyValue { get; private set; } = default!;

        protected override void OnCondiition(object model, IReadOnlyList<bool> flags)
        {
            if (flags.Any(p => p))
            {
                var value = LazyValue.Value;
                // Clear the value by resetting to default
                PropertyGridItem.Property.SetValue(model, value);
                // Also update the UI control if necessary
                PropertyGridItem.InputControl.CrossThread(() => {
                    Property.SetValue(model, value);
                    PropertyGridItem.SetValue(value);
                });
            }
        }

        protected override void OnInitialize(object model)
        {
            base.OnInitialize(model);

            LazyValue = new Lazy<object?>(ModelType.GetDefault);
        }
    }

    protected override IWatchHandler? OnCreateInstance(PropertyGridItem gridItem, ClearWhenAttribute[] attributes) => new ClearWhenHandlerInstance(gridItem, attributes);
}