using CoreOne.Winforms.Transitions;

namespace CoreOne.Winforms.Extensions;

public static class TransitionExtensions
{
    public static Transition ToTransition(this ITransitionType type) => new(type);
}