using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;

namespace CoreOne.Winforms.Services.PropertyControlFactories;

/// <summary>
/// Factory for creating dropdown controls for string properties with DropdownSourceAttribute
/// </summary>
public class DropdownControlFactory(IServiceProvider services, IDropdownRefreshManager refreshManager) : IPropertyControlFactory
{
    public bool CanHandle(Metadata property) => property.GetCustomAttribute<DropdownSourceAttribute>() != null;

    public (Control? control, Action<object?>? setValue) CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var provider = GetProvider(property);
        if (provider == null)
            return (null, null);

        var context = new DropdownContext(provider, property);
        context.ComboBox.SelectedIndexChanged += (s, e) => {
            if (context.ComboBox.SelectedItem is DropdownItem selectedItem)
                onValueChanged(selectedItem.Value);
        };

        // Register the dropdown for refresh management
        refreshManager.RegisterDropdown(context, model);
        return (context.ComboBox, UpdateControlValue);

        void UpdateControlValue(object? value)
        {
            var comboBox = context.ComboBox;
            foreach (var item in comboBox.Items)
            {
                if (item is DropdownItem dropdownItem && dropdownItem.Value?.Equals(value) == true)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }
    }

    private IDropdownSourceProvider? GetProvider(Metadata property)
    {
        var attribute = property.GetCustomAttribute<DropdownSourceAttribute>();
        if (attribute == null)
            return null;

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