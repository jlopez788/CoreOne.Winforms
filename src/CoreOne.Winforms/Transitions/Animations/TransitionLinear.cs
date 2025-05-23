namespace CoreOne.Winforms.Transitions.Animations;

public class TransitionLinear(int duration) : Animation(duration)
{
    public override void OnTimer(int ts, out double percentage, out bool bCompleted)
    {
        percentage = ts / Duration;
        bCompleted = false;
        if (percentage >= 1.0)
        {
            percentage = 1.0;
            bCompleted = true;
        }
    }
}