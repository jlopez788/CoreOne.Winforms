namespace CoreOne.Winforms.Models;

public abstract class ControlContext(Control control, Action<object?> setValue) : Disposable
{
    public Control Control { get; } = control;
    protected SToken Token { get; set; } = SToken.Create();

    public void BindEvent()
    {
        Token.Dispose();
        OnBindEvent();
    }

    public void UnbindEvent() => OnUnbindEvent();

    public void UpdateValue(object? currentValue) => setValue.Invoke(currentValue);

    protected virtual void OnBindEvent()
    {
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        OnUnbindEvent();
    }

    protected virtual void OnUnbindEvent() => Token.Dispose();
}