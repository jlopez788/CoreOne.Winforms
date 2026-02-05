using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Services.WatchHandlers;

public class DropdownHandler(IServiceProvider services) : WatchFactoryFromAttribute<DropdownSourceAttribute>
{
    private class DropdownHanderInstance(IDropdownSourceProvider provider, PropertyGridItem gridItem) : WatchHandler(gridItem.Property), IWatchHandler
    {
        private readonly ComboBox ComboBox = (ComboBox)gridItem.InputControl;

        protected override void OnInitialize(object model) => provider.Initialize(this);

        protected override void OnRefresh(object model, bool isFirst) => _ = RefreshItemsAsync(model);

        private void RefreshItems(IEnumerable<DropdownItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            var itemList = items.ToList();
            ComboBox.CrossThread(() => {
                var previousValue = ComboBox.SelectedItem is DropdownItem item ? item.Value : null;

                ComboBox.DataSource = itemList;
                ComboBox.DisplayMember = nameof(DropdownItem.Display);
                RestoreSelection(previousValue, itemList);
            });
        }

        private async Task RefreshItemsAsync(object model)
        {
            if (provider is IDropdownSourceProviderSync syncProvider)
            {
                var items = syncProvider.GetItems(model);
                RefreshItems(items);
            }
            else if (provider is IDropdownSourceProviderAsync asyncProvider)
            {
                var items = await asyncProvider.GetItemsAsync(model);
                RefreshItems(items);
            }
        }

        private void RestoreSelection(object? previousValue, List<DropdownItem> itemList)
        {
            // Try to restore previous selection
            if (previousValue != null)
            {
                foreach (var dropdownItem in itemList)
                {
                    if (dropdownItem.Value == previousValue)
                    {
                        ComboBox.SelectedItem = dropdownItem;
                        return;
                    }
                }
            }

            // Select first item if available
            if (ComboBox.Items.Count > 0)
            {
                ComboBox.SelectedIndex = 0;
            }
        }
    }

    protected override bool CanHandle(PropertyGridItem gridItem, DropdownSourceAttribute[] attributes) => attributes.Length > 0 && gridItem.InputControl is ComboBox;

    protected override IWatchHandler? OnCreateInstance(PropertyGridItem gridItem, DropdownSourceAttribute[] attributes)
    {
        var provider = GetProvider(attributes[0]);
        return provider is null ? null : new DropdownHanderInstance(provider, gridItem);
    }

    private IDropdownSourceProvider? GetProvider(DropdownSourceAttribute attribute)
    {
        try
        {
            // Try to resolve from service provider first
            var instance = services.GetService(attribute.SourceType);
            instance ??= services.GetTargetCreator().CreateInstance(attribute.SourceType);

            return instance as IDropdownSourceProvider;
        }
        catch
        {
            return null;
        }
    }
}