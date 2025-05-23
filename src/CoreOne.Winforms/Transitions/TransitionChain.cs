namespace CoreOne.Winforms.Transitions;

internal class TransitionChain
{
    private readonly List<Transition> _Transitions;
    public CancellationToken CancellationToken { get; }

    public TransitionChain(Transition[] transitions)
    {
        _Transitions = [.. transitions];
        Run();
    }

    public TransitionChain(Transition[] transitions, CancellationToken cancellationToken) : this(transitions) => CancellationToken = cancellationToken;

    private void OnTransitionCompleted(object? sender, EventArgs e)
    {
        if (sender is Transition transition)
        {
            transition.TransitionCompletedEvent -= OnTransitionCompleted;
            _Transitions.RemoveAt(0);
            Run();
        }
    }

    private void Run()
    {
        if (_Transitions.Count > 0)
        {
            var nextTransition = _Transitions[0];
            nextTransition.TransitionCompletedEvent += OnTransitionCompleted;
            nextTransition.Run(CancellationToken);
        }
    }
}