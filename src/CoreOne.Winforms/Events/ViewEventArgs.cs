namespace CoreOne.Winforms.Events;

public record ViewEventArgs(ViewActionType viewActionType, string name = "")
{
    public bool AddToHistory { get; init; } = true;
    public bool Animate { get; init; } = true;
    public IReadOnlyList<object> Args { get; init; } = [];
    public bool Cancel { get; set; }
    public string Name { get; init; } = name;
    public ViewActionType ViewActionType { get; init; } = viewActionType;

    public ViewEventArgs() : this(string.Empty)
    {
        Args = [];
    }

    public ViewEventArgs(string name, params object[] args) : this(ViewActionType.GoToNamedView, name)
    {
        Args = [.. args];
    }

    public override int GetHashCode() => (Name, ViewActionType, Args).GetHashCode();
}