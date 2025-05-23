namespace CoreOne.Winforms.Transitions.Animations;

public class TransitionAcceleration(int duration) : Animation(duration)
{
    public override void OnTimer(int ts, out double percentage, out bool completed)
    {
        double dElapsed = ts / Duration;
        percentage = dElapsed * dElapsed;
        completed = false;
        if (dElapsed >= 1.0)
        {
            percentage = 1.0;
            completed = true;
        }
    }
}