namespace CoreOne.Winforms.Events;

public sealed class ModelSavedEventArgs(object target) : EventArgs
{
    public bool IsModified { get; init; }
    public bool IsValid => Validation.IsValid;
    public object Target { get; } = target;
    public MValidationResult Validation { get; init; }
}