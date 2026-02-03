namespace CoreOne.Winforms.Services.ControlFactories;

public class DateTimeControlFactory : IControlFactory
{
    public bool CanHandle(Metadata property)
    {
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return underlyingType == typeof(DateTime);
    }

    public (Control control, Action<object?> setValue)? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var dateTimePicker = new DateTimePicker();
        dateTimePicker.ValueChanged += (s, e) => onValueChanged(dateTimePicker.Value);
        return (dateTimePicker, UpdateControlValue);

        void UpdateControlValue(object? value)
        {
            dateTimePicker.Value = (DateTime)(value ?? DateTime.Now);
        }
    }
}