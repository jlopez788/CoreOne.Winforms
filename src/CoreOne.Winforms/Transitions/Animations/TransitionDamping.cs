namespace CoreOne.Winforms.Transitions.Animations;

public class TransitionDamping(int duration) : Animation(duration)
{
    public override void OnTimer(int ts, out double percentage, out bool bCompleted)
    {
        double dElapsed = ts / Duration;
        percentage = (1.0 - Math.Exp(-1.0 * dElapsed * 5)) / 0.993262053;
        bCompleted = false;
        if (dElapsed >= 1.0)
        {
            percentage = 1.0;
            bCompleted = true;
        }
    }
}