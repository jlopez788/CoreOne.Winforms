namespace CoreOne.Winforms.Services.ControlFactories;

public class EnumControlFactory : DropdownControlFactory, IControlFactory
{
    protected override bool OnCanHandle(Metadata property)
    {
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return underlyingType.IsEnum;
    }

    protected override (ComboBox control, Action<object?> setValue)? OnCreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var result = base.OnCreateControl(property, model, onValueChanged);
        if (result is null)
            return null;

        var combo = result.Value.control;
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        combo.Items.AddRange([.. Enum.GetValues(underlyingType).Cast<object>()]);
        return (control: combo, setValue: value => {
            if (value != null)
            {
                combo.SelectedItem = value;
            }
        }
        );
    }
}