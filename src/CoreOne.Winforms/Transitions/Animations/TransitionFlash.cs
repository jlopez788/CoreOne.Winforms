namespace CoreOne.Winforms.Transitions.Animations;

public class TransitionFlash : TransitionUserDefined
{
    public TransitionFlash(int flashes, int flashtime)
    {
        double dFlashInterval = 100.0 / flashes;
        var elements = new List<TransitionElement>(flashes);
        for (int i = 0; i < flashes; ++i)
        {
            double dFlashStartTime = i * dFlashInterval;
            double dFlashEndTime = dFlashStartTime + dFlashInterval;
            double dFlashMidPoint = (dFlashStartTime + dFlashEndTime) / 2.0;
            elements.Add(new TransitionElement(dFlashMidPoint, 100, InterpolationMethod.EaseInEaseOut));
            elements.Add(new TransitionElement(dFlashEndTime, 0, InterpolationMethod.EaseInEaseOut));
        }
        Duration = flashes * flashtime;
        Elements.AddRange(elements);
    }
}