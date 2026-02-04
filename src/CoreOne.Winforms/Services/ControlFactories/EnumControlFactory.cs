namespace CoreOne.Winforms.Services.ControlFactories;

public class EnumControlFactory : DropdownControlFactory, IControlFactory
{
    protected override bool OnCanHandle(Metadata property)
    {
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return underlyingType.IsEnum;
    }

    protected override ControlContext? OnCreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var result = base.OnCreateControl(property, model, onValueChanged);
        if (result is null)
            return null;

        var combo = (ComboBox)result.Control;
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        combo.Items.AddRange([.. Enum.GetValues(underlyingType).Cast<object>()]);
        return new(combo, value => {
            if (value != null)
            {
                combo.SelectedItem = value;
            }
        }, result.BindEvent);
    }
}