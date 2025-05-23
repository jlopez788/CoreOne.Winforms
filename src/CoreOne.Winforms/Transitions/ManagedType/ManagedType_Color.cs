namespace CoreOne.Winforms.Transitions.ManagedType;

internal class ManagedType_Color : IManagedType
{
    public Type ManagedType => typeof(Color);

    public object Copy(object o)
    {
        var c = (Color)o;
        var result = Color.FromArgb(c.ToArgb());
        return result;
    }

    public object IntermediateValue(object start, object end, double percentage)
    {
        var startColor = (Color)start;
        var endColor = (Color)end;

        int iStart_R = startColor.R;
        int iStart_G = startColor.G;
        int iStart_B = startColor.B;
        int iStart_A = startColor.A;

        int iEnd_R = endColor.R;
        int iEnd_G = endColor.G;
        int iEnd_B = endColor.B;
        int iEnd_A = endColor.A;

        int new_R = Utility.Interpolate(iStart_R, iEnd_R, percentage);
        int new_G = Utility.Interpolate(iStart_G, iEnd_G, percentage);
        int new_B = Utility.Interpolate(iStart_B, iEnd_B, percentage);
        int new_A = Utility.Interpolate(iStart_A, iEnd_A, percentage);

        return Color.FromArgb(new_A, new_R, new_G, new_B);
    }
}