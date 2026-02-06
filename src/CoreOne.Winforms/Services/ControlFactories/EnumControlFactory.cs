namespace CoreOne.Winforms.Services.ControlFactories;

public class EnumControlFactory : IControlFactory
{
    public bool CanHandle(Metadata property)
    {
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return underlyingType.IsEnum;
    }

    public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var combo = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        combo.Items.AddRange([.. Enum.GetValues(underlyingType).Cast<object>()]);

        return new EventControlContext(combo,
            nameof(combo.SelectedIndexChanged),
            value => {
                if (value != null)
                {
                    combo.SelectedItem = value;
                }
            },
            () => onValueChanged(combo.SelectedItem)
        );
    }
}