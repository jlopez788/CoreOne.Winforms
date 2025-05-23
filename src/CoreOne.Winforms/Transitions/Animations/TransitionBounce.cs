namespace CoreOne.Winforms.Transitions.Animations;

public class TransitionBounce : TransitionUserDefined
{
    public TransitionBounce(int duration) : base(duration)
    {
        Elements.Add(new TransitionElement(50, 100, InterpolationMethod.Accleration));
        Elements.Add(new TransitionElement(100, 0, InterpolationMethod.Deceleration));
    }
}