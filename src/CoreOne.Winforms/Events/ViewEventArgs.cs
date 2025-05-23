namespace CoreOne.Winforms.Events;

public record ViewEventArgs(ViewActionType ViewActionType, string Name = "")
{
    public bool AddToHistory { get; init; } = true;
    public bool Animate { get; init; } = true;
    public IReadOnlyList<object> Args { get; init; } = [];
    public bool Cancel { get; set; }

    public ViewEventArgs() : this(string.Empty)
    {
        Args = [];
    }

    public ViewEventArgs(string name, params object[] args) : this(ViewActionType.GoToNamedView, name)
    {
        Args = [.. args];
    }
}