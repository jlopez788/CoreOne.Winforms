namespace CoreOne.Winforms.Services.PropertyControlFactories;

public class StringControlFactory : IPropertyControlFactory
{
    public bool CanHandle(Metadata property)
    {
        var propertyType = property.FPType;
        return propertyType == Types.String;
    }

    public (Control control, Action<object?> setValue)? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var textBox = new TextBox();
        textBox.TextChanged += (s, e) => onValueChanged(textBox.Text);
        return (textBox, UpdateControlValue);

        void UpdateControlValue(object? value) => textBox.Text = value?.ToString() ?? string.Empty;
    }
}