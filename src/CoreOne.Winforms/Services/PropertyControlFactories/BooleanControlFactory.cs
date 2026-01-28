namespace CoreOne.Winforms.Services.PropertyControlFactories;

public class BooleanControlFactory : IPropertyControlFactory
{
    public bool CanHandle(Metadata property)
    {
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return underlyingType == typeof(bool);
    }

    public (Control? control, Action<object?>? setValue) CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var checkBox = new CheckBox();
        checkBox.CheckedChanged += (s, e) => onValueChanged(checkBox.Checked);
        return (checkBox, UpdateControlValue);

        void UpdateControlValue(object? value)
        {
            checkBox.Checked = value is bool flag && flag;
        }
    }
}