using CoreOne.Attributes;
using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Models;

public class EnabledContext(Control control, Metadata property, EnabledWhenAttribute attr) : WatchContext(property)
{
    public Control Control { get; } = control ?? throw new ArgumentNullException(nameof(control));
    protected EnabledWhenAttribute Attribute { get; } = attr ?? throw new ArgumentNullException(nameof(attr));
    protected Metadata TargetProperty { get; private set; }

    protected override void OnInitialized(object model)
    {
        TargetProperty = MetaType.GetMetadata(model.GetType(), Attribute.PropertyName);
    }

    protected override void OnRefresh(object model)
    {
        var targetValue = TargetProperty.GetValue(model);
        Control.CrossThread(() => {
            var value = Property.GetValue(model);
            var comparable = typeof(IComparable);
            var sourceValue = value as IComparable;
            var isSVNull = sourceValue == null;
            var isTVNull = targetValue == null;
            var isEnabled = Attribute.ComparisonType switch {
                ComparisonType.LessThan => !isSVNull && !isTVNull && sourceValue?.CompareTo((IComparable?)targetValue) < 0,
                ComparisonType.LessThanOrEqualTo => !isSVNull && !isTVNull && sourceValue?.CompareTo((IComparable?)targetValue) <= 0,
                ComparisonType.NotEqualTo => (!isSVNull && isTVNull) || (isSVNull && !isTVNull) || (!isSVNull && !isTVNull && sourceValue?.CompareTo((IComparable?)targetValue) != 0),
                ComparisonType.EqualTo => (isSVNull && isTVNull) || (!isSVNull && !isTVNull && sourceValue?.CompareTo((IComparable?)targetValue) == 0),
                ComparisonType.GreaterThan => !isSVNull && !isTVNull && sourceValue?.CompareTo((IComparable?)targetValue) > 0,
                ComparisonType.GreaterThanOrEqualTo => !isSVNull && !isTVNull && sourceValue?.CompareTo((IComparable?)targetValue) >= 0,
                _ => throw new InvalidOperationException(),
            };

            if (value is not null && !comparable.IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException("value has not implemented IComparable interface");
            }

            Control.Enabled = isEnabled;
        });
    }
}

/// <summary>
/// Implementation of dropdown context for provider initialization
/// </summary>
public class DropdownContext : WatchContext
{
    public ComboBox ComboBox { get; }
    public IDropdownSourceProvider Provider { get; }

    public DropdownContext(IDropdownSourceProvider provider, Metadata property) : base(property)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ComboBox = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList
        };
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

    protected override void OnInitialized(object model) => Provider.Initialize(this);

    protected override void OnRefresh(object model) => _ = RefreshItemsAsync(model);

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