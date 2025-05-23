namespace CoreOne.Winforms.Transitions.ManagedType;

internal class ManagedType_Float : IManagedType
{
    public Type ManagedType => typeof(float);

    public object Copy(object o)
    {
        float f = (float)o;
        return f;
    }

    public object IntermediateValue(object start, object end, double percentage)
    {
        float fStart = (float)start;
        float fEnd = (float)end;
        return Utility.Interpolate(fStart, fEnd, percentage);
    }
}