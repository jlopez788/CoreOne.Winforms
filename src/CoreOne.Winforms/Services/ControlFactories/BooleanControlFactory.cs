namespace CoreOne.Winforms.Services.ControlFactories;

public class BooleanControlFactory : IControlFactory
{
    public bool CanHandle(Metadata property)
    {
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return underlyingType == typeof(bool);
    }

    public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var checkBox = new CheckBox();
        return new(checkBox,
            value => checkBox.Checked = value is bool flag && flag,
            () => checkBox.CheckedChanged += (s, e) => onValueChanged(checkBox.Checked));
    }
}