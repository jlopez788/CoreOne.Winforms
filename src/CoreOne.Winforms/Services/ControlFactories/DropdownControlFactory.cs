using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Services.ControlFactories;

/// <summary>
/// Factory for creating dropdown controls for string properties with DropdownSourceAttribute
/// </summary>
public class DropdownControlFactory : IControlFactory
{
    /// <summary>
    /// High priority (100) to ensure attribute-based factories take precedence over generic type-based factories
    /// </summary>
    public int Priority => 100;

    public bool CanHandle(Metadata property) => property.GetCustomAttribute<DropdownSourceAttribute>() != null;

    public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var dropdown = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        return new(dropdown,
            nameof(dropdown.SelectedIndexChanged),
            value => {
                foreach (var item in dropdown.Items)
                {
                    if (item is DropdownItem dropdownItem && dropdownItem.Value?.Equals(value) == true)
                    {
                        dropdown.SelectedItem = item;
                        return;
                    }
                }
            },
            () => {
                if (dropdown.SelectedItem is DropdownItem selectedItem)
                    onValueChanged(selectedItem.Value);
            }
        );
    }
}