namespace CoreOne.Winforms.Models;

public readonly struct History(string viewName, IReadOnlyList<object> args) : IEquatable<History>
{
    private readonly Guid Id = Guid.NewGuid();
    public IReadOnlyList<object> Args { get; } = args;
    public string ViewName { get; } = viewName;

    public static bool operator !=(History left, History right) => !(left == right);

    public static bool operator ==(History left, History right) => left.Equals(right);

    public override bool Equals(object? obj) => obj is History history && Id == history.Id;

    public bool Equals(History other) => Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}