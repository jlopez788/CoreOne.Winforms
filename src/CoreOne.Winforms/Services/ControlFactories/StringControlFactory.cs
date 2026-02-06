namespace CoreOne.Winforms.Services.ControlFactories;

public class StringControlFactory : IControlFactory
{
    public bool CanHandle(Metadata property)
    {
        var propertyType = property.FPType;
        return propertyType == Types.String;
    }

    public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var textBox = new TextBox {
            Name = property.Name
        };

        return new EventControlContext(textBox,
            nameof(textBox.TextChanged),
            value => textBox.Text = value?.ToString(),
            () => onValueChanged(textBox.Text));
    }
}