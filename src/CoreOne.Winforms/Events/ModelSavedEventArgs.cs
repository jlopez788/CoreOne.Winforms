namespace CoreOne.Winforms.Events;

public sealed class ModelSavedEventArgs(object target) : EventArgs
{
    public object Target { get; } = target;
}