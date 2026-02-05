using CoreOne.Reactive;

namespace CoreOne.Winforms.Models;

public class ControlContext(Control control, string eventName, Action<object?> setValue, Action controlChanged)
{
    private SToken Token = SToken.Create();
    public Control Control { get; } = control;

    public void BindEvent()
    {
        Token.Dispose();
        if (!string.IsNullOrEmpty(eventName))
        {
            Token = SToken.Create();
            Observable.FromEvent<EventArgs>(Control, eventName)
                .Subscribe(p => controlChanged?.Invoke(), Token);
        }
    }

    public void UnbindEvent() => Token.Dispose();

    public void UpdateValue(object? currentValue) => setValue.Invoke(currentValue);
}