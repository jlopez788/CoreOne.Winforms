namespace CoreOne.Winforms.Services.PropertyControlFactories;

public class EnumControlFactory : IPropertyControlFactory
{
    public bool CanHandle(Metadata property)
    {
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return underlyingType.IsEnum;
    }

    public (Control control, Action<object?> setValue)? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        var comboBox = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        comboBox.Items.AddRange([.. Enum.GetValues(underlyingType).Cast<object>()]);
        comboBox.SelectedIndexChanged += (s, e) => {
            if (comboBox.SelectedItem != null)
                onValueChanged(comboBox.SelectedItem);
        };
        return (comboBox, UpdateControlValue);

        void UpdateControlValue(object? value)
        {
            if (value != null)
            {
                comboBox.SelectedItem = value;
            }
        }
    }
}