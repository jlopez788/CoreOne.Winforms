namespace CoreOne.Winforms.Transitions.ManagedType;

internal class ManagedType_Int : IManagedType
{
    public Type ManagedType => typeof(int);

    public object Copy(object o)
    {
        int value = (int)o;
        return value;
    }

    public object IntermediateValue(object start, object end, double percentage)
    {
        int iStart = (int)start;
        int iEnd = (int)end;
        return Utility.Interpolate(iStart, iEnd, percentage);
    }
}