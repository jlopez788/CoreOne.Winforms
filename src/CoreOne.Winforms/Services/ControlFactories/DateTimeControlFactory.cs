namespace CoreOne.Winforms.Services.ControlFactories;

public class DateTimeControlFactory : IControlFactory
{
    public bool CanHandle(Metadata property)
    {
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return underlyingType == typeof(DateTime);
    }

    public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var dateTimePicker = new DateTimePicker();
        return new(dateTimePicker,
            value => dateTimePicker.Value = (DateTime)(value ?? DateTime.Now),
            () => dateTimePicker.ValueChanged += (s, e) => onValueChanged(dateTimePicker.Value));
    }
}