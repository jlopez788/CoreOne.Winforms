namespace CoreOne.Winforms.Transitions.Animations;

public class TransitionEaseInEaseOut(int duration) : Animation(duration)
{
    public override void OnTimer(int ts, out double percentage, out bool bCompleted)
    {
        double dElapsed = ts / Duration;
        percentage = Utility.ConvertLinearToEaseInEaseOut(dElapsed);
        bCompleted = false;
        if (dElapsed >= 1.0)
        {
            percentage = 1.0;
            bCompleted = true;
        }
    }
}