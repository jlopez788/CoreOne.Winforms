namespace CoreOne.Winforms.Models;

public class ControlContext(Control control, Action<object?> setValue, Action? bindEvent)
{
    public Control Control { get; } = control;

    public void BindEvent() => bindEvent?.Invoke();

    public void UpdateValue(object? currentValue) => setValue.Invoke(currentValue);
}