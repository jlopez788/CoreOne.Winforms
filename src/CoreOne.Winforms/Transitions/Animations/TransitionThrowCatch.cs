namespace CoreOne.Winforms.Transitions.Animations;

public class TransitionThrowCatch : TransitionUserDefined
{
    public TransitionThrowCatch(int duration) : base(duration)
    {
        Elements.Add(new TransitionElement(50, 100, InterpolationMethod.Deceleration));
        Elements.Add(new TransitionElement(100, 0, InterpolationMethod.Accleration));
    }
}