using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;

namespace CoreOne.Winforms.Services;

/// <summary>
/// Implementation of dropdown context for provider initialization
/// </summary>
public class DropdownContext : IDropdownContext
{
    public ComboBox ComboBox { get; }
    public Metadata Property { get; }
    public IDropdownSourceProvider Provider { get; }
    public HashSet<string> Dependencies { get; } = [];

    public DropdownContext(IDropdownSourceProvider provider, Metadata property)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Property = property;
        ComboBox = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        Dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Dependencies.AddRange(property.GetCustomAttributes<DropdownDependsOnAttribute>()
            .Select(attr => attr.PropertyName));
    }

    public void RefreshItems(IEnumerable<DropdownItem> items)
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

    public void RequestRefresh(object model) => _ = RefreshItemsAsync(model);

    private async Task RefreshItemsAsync(object model)
    {
        if (Provider is IDropdownSourceProviderAsync asyncProvider)
        {
            var items = await asyncProvider.GetItemsAsync(model);
            RefreshItems(items);
        }
        else if (Provider is IDropdownSourceProviderSync syncProvider)
        {
            var items = syncProvider.GetItems(model);
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