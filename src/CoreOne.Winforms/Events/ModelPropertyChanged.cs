namespace CoreOne.Winforms.Events;

public class ModelPropertyChanged(Metadata property, object target, object? newValue)
{
    public Metadata Property { get; } = property;
    public object? NewValue { get; } = newValue;
    public object Target { get; } = target;
}
