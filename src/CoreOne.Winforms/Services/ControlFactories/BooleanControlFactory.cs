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
        return new EventControlContext(checkBox,
            nameof(checkBox.CheckedChanged),
            value => checkBox.Checked = value is bool flag && flag,
            () => onValueChanged(checkBox.Checked));
    }
}