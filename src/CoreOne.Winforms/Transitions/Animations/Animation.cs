namespace CoreOne.Winforms.Transitions.Animations;

public abstract class Animation(int duration) : ITransitionType
{
    public double Duration { get; protected set; } = duration;

    public static ITransitionType Acceleration(int duration) => new TransitionAcceleration(duration);

    public static ITransitionType Bounce(int duration) => new TransitionBounce(duration);

    public static ITransitionType Damping(int duration) => new TransitionDamping(duration);

    public static ITransitionType Deceleration(int duration) => new TransitionDeceleration(duration);

    public static ITransitionType EaseInEaseOut(int duration) => new TransitionEaseInEaseOut(duration);

    public static ITransitionType Flash(int flashes, int flashtime) => new TransitionFlash(flashes, flashtime);

    public static ITransitionType Linear(int duration) => new TransitionLinear(duration);

    public static ITransitionType ThrowCatch(int duration) => new TransitionThrowCatch(duration);

    public static ITransitionType UserDefined(IEnumerable<TransitionElement> elements, int duration) => new TransitionUserDefined(elements, duration);

    public abstract void OnTimer(int ts, out double percentage, out bool completed);
}