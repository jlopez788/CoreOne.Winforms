using System.Diagnostics.CodeAnalysis;

namespace CoreOne.Winforms.Models;

public readonly struct Priority : IComparable<Priority>
{
    public static readonly Priority Default = new(0);
    public static readonly Priority Max = new(int.MaxValue);

    public int Value { get; }

    public Priority(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, int.MaxValue);
        Value = value;
    }

    public static implicit operator int(Priority p) => p.Value;

    public static bool operator !=(Priority left, Priority right) => !(left == right);

    public static bool operator <(Priority left, Priority right) => left.Value < right.Value;

    public static bool operator <=(Priority left, Priority right) => left.Value <= right.Value;

    public static bool operator ==(Priority left, Priority right) => left.Equals(right);

    public static bool operator >(Priority left, Priority right) => left.Value > right.Value;

    public static bool operator >=(Priority left, Priority right) => left.Value >= right.Value;

    public int CompareTo(Priority other) => Value.CompareTo(other.Value);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Priority p && Value == p.Value;

    public override int GetHashCode() => Value;

    public override string ToString() => $"P: {Value}";
}