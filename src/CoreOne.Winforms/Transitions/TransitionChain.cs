namespace CoreOne.Winforms.Transitions;

internal class TransitionChain
{
    private readonly List<Transition> _Transitions;
    public CancellationToken CancellationToken { get; }
    private SToken Token = SToken.Create();

    public TransitionChain(Transition[] transitions)
    {
        _Transitions = [.. transitions];
        Run();
    }

    public TransitionChain(Transition[] transitions, CancellationToken cancellationToken) : this(transitions) => CancellationToken = cancellationToken;

    private void OnTransitionCompleted()
    {
        _Transitions.RemoveAt(0);
        Run();
    }

    private void Run()
    {
        Token.Dispose();
        if (_Transitions.Count > 0)
        {
            var nextTransition = _Transitions[0];
            Token = SToken.Create();

            nextTransition
                .OnComplete(OnTransitionCompleted, Token)
                .Run(CancellationToken);
        }
    }
}