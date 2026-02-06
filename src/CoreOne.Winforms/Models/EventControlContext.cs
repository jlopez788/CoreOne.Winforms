using CoreOne.Reactive;

namespace CoreOne.Winforms.Models;

public class EventControlContext(Control control, string eventName, Action<object?> setValue, Action controlChanged) : ControlContext(control, setValue)
{
    protected override void OnBindEvent()
    {
        if (!string.IsNullOrEmpty(eventName))
        {
            Token = SToken.Create();
            Observable.FromEvent<EventArgs>(Control, eventName)
                .Subscribe(p => controlChanged?.Invoke(), Token);
        }
    }
}