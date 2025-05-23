namespace CoreOne.Winforms.Transitions.Animations;

public class TransitionUserDefined : Animation
{
    protected List<TransitionElement> Elements;
    protected int m_iCurrentElement = 0;

    public TransitionUserDefined() : this(0)
    {
    }

    public TransitionUserDefined(int duration)
        : base(duration) => Elements = [];

    public TransitionUserDefined(IEnumerable<TransitionElement> elements, int duration)
        : base(duration)
    {
        Elements = [.. elements];
    }

    public override void OnTimer(int ts, out double percentage, out bool bCompleted)
    {
        double dTransitionTimeFraction = ts / Duration;

        GetElementInfo(dTransitionTimeFraction, out var dElementStartTime, out var dElementEndTime, out var dElementStartValue, out var dElementEndValue, out var eInterpolationMethod);

        double dElementInterval = dElementEndTime - dElementStartTime;
        double dElementElapsedTime = dTransitionTimeFraction - dElementStartTime;
        double dElementTimeFraction = dElementElapsedTime / dElementInterval;
        var dElementDistance = eInterpolationMethod switch {
            InterpolationMethod.Linear => dElementTimeFraction,
            InterpolationMethod.Accleration => Utility.ConvertLinearToAcceleration(dElementTimeFraction),
            InterpolationMethod.Deceleration => Utility.ConvertLinearToDeceleration(dElementTimeFraction),
            InterpolationMethod.EaseInEaseOut => Utility.ConvertLinearToEaseInEaseOut(dElementTimeFraction),
            _ => throw new Exception("Interpolation method not handled: " + eInterpolationMethod.ToString()),
        };
        percentage = Utility.Interpolate(dElementStartValue, dElementEndValue, dElementDistance);
        bCompleted = false;
        if (ts >= Duration)
        {
            bCompleted = true;
            percentage = dElementEndValue;
        }
    }

    private void GetElementInfo(double dTimeFraction, out double dStartTime, out double dEndTime, out double dStartValue, out double dEndValue, out InterpolationMethod eInterpolationMethod)
    {
        int iCount = Elements.Count;
        for (; m_iCurrentElement < iCount; ++m_iCurrentElement)
        {
            var element = Elements[m_iCurrentElement];
            double dElementEndTime = element.EndTime / 100.0;
            if (dTimeFraction < dElementEndTime)
                break;
        }

        if (m_iCurrentElement == iCount)
            m_iCurrentElement = iCount - 1;

        dStartTime = 0.0;
        dStartValue = 0.0;
        if (m_iCurrentElement > 0)
        {
            var previousElement = Elements[m_iCurrentElement - 1];
            dStartTime = previousElement.EndTime / 100.0;
            dStartValue = previousElement.EndValue / 100.0;
        }

        var currentElement = Elements[m_iCurrentElement];
        dEndTime = currentElement.EndTime / 100.0;
        dEndValue = currentElement.EndValue / 100.0;
        eInterpolationMethod = currentElement.InterpolationMethod;
    }
}