using CoreOne.Drawing.Models;

namespace CoreOne.Winforms.Transitions.ManagedType;

internal class ManagedType_GradientColor : IManagedType
{
    private static readonly ManagedType_Color ColorLerp = new();
    public Type ManagedType => typeof(GColor);

    public object Copy(object o) => o is GColor g ? new GColor(g.Start, g.End) : GColor.Empty;

    public object IntermediateValue(object start, object end, double percentage)
    {
        var startColor = (GColor)start;
        var endColor = (GColor)end;

        var middleStart = (Color)ColorLerp.IntermediateValue(startColor.Start, endColor.Start, percentage);
        Color? middleEnd = null;
        if (endColor.End.HasValue && startColor.End.HasValue)
            middleEnd = (Color)ColorLerp.IntermediateValue(startColor.End, endColor.End, percentage);
        return new GColor(middleStart, middleEnd);
    }
}