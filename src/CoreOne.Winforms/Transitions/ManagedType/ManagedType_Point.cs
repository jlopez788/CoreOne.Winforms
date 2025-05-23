namespace CoreOne.Winforms.Transitions.ManagedType;

internal class ManagedType_Point : IManagedType
{
    public Type ManagedType => typeof(Point);

    public object Copy(object o)
    {
        var pt = (Point)o;
        return new Point(pt.X, pt.Y);
    }

    public object IntermediateValue(object start, object end, double percentage)
    {
        Point
            pts = (Point)start,
            pte = (Point)end;
        int x = Utility.Interpolate(pts.X, pte.X, percentage);
        int y = Utility.Interpolate(pts.Y, pte.Y, percentage);
        return new Point(x, y);
    }
}