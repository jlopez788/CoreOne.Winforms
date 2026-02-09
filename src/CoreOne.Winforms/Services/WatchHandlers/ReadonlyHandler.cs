using System.ComponentModel;

namespace CoreOne.Winforms.Services.WatchHandlers;

public class ReadonlyHandler : WatchFactoryFromAttribute<ReadOnlyAttribute>, IWatchFactory
{
    private class ReadonlyHandlerInstance(PropertyGridItem gridItem, ReadOnlyAttribute[] attributes) : WatchHandler(gridItem.Property)
    {
        protected override void OnInitialize(object model)
        {
            var isreadonly = attributes.Any(p => p.IsReadOnly);
            gridItem.InputControl.Enabled = !isreadonly;
            if (isreadonly)
            {
                Dependencies.Clear();
            }
        }
    }

    protected override IWatchHandler? OnCreateInstance(PropertyGridItem gridItem, ReadOnlyAttribute[] attributes) => new ReadonlyHandlerInstance(gridItem,attributes);
}