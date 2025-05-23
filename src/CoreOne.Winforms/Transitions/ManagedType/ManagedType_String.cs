namespace CoreOne.Winforms.Transitions.ManagedType;

internal class ManagedType_String : IManagedType
{
    public Type ManagedType => Types.String;

    public object Copy(object o)
    {
        string s = (string)o;
        return new string(s.ToCharArray());
    }

    public object IntermediateValue(object start, object end, double percentage)
    {
        string strStart = (string)start;
        string strEnd = (string)end;

        int iStartLength = strStart.Length;
        int iEndLength = strEnd.Length;
        int iLength = Utility.Interpolate(iStartLength, iEndLength, percentage);
        char[] result = new char[iLength];

        for (int i = 0; i < iLength; ++i)
        {
            char cStart = 'a';
            if (i < iStartLength)
                cStart = strStart[i];

            char cEnd = 'a';
            if (i < iEndLength)
                cEnd = strEnd[i];

            char cInterpolated;
            if (cEnd == ' ')
                cInterpolated = ' ';
            else
            {
                int iStart = Convert.ToInt32(cStart);
                int iEnd = Convert.ToInt32(cEnd);
                int iInterpolated = Utility.Interpolate(iStart, iEnd, percentage);
                cInterpolated = Convert.ToChar(iInterpolated);
            }
            result[i] = cInterpolated;
        }

        return new string(result);
    }
}