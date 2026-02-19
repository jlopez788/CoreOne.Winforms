using System.Diagnostics;

namespace CoreOne.Winforms.Models;

[DebuggerDisplay("{Display}")]
public class GroupDetail(int groupId, string title, int priority = 0, GridColumnSpan columnSpan = GridColumnSpan.Full)
{
    public const int GROUP_ID = 0;
    public static readonly GroupDetail Default = new(GROUP_ID, "Default", int.MaxValue);
    public GridColumnSpan ColumnSpan { get; } = columnSpan;
    public int GroupId { get; } = groupId;
    public int Priority { get; } = priority;
    public string Title { get; } = title;
    private string Display => $"[{(int)ColumnSpan}] {Title} :: {GroupId}... Priority: {(Priority == int.MaxValue ? "MAX" : Priority)}";
}