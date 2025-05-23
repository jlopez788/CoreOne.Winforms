namespace CoreOne.Winforms.Transitions.ManagedType;

internal class ManagedType_Double : IManagedType
{
    public Type ManagedType => typeof(double);

    public object Copy(object o)
    {
        double d = (double)o;
        return d;
    }

    public object IntermediateValue(object start, object end, double percentage)
    {
        double dStart = (double)start;
        double dEnd = (double)end;
        return Utility.Interpolate(dStart, dEnd, percentage);
    }
}