namespace CoreOne.Winforms.Transitions;

internal class TransitionManager
{
    private readonly Lock Lock = new();
    private readonly Timer? Timer = null;
    private readonly Dictionary<Transition, bool> Transitions = new(50);
    public static TransitionManager Instance { get; } = new();

    private TransitionManager()
    {
        Timer = new Timer {
            Interval = 10
        };
        Timer.Tick += OnTimerElapsed;
        Timer.Enabled = true;
    }

    public void Register(Transition transition)
    {
        lock (Lock)
        {
            RemoveDuplicates(transition);
            Transitions[transition] = true;
            transition.TransitionCompletedEvent += OnTransitionCompleted;
        }
    }

    private static void RemoveDuplicates(Transition newTransition, Transition oldTransition)
    {
        var newProperties = newTransition.TransitionedProperties;
        var oldProperties = oldTransition.TransitionedProperties;
        for (int i = oldProperties.Count - 1; i >= 0; i--)
        {
            var oldProperty = oldProperties[i];
            foreach (var newProperty in newProperties)
            {
                if (oldProperty.Target == newProperty.Target && oldProperty.Metadata == newProperty.Metadata)
                    oldTransition.RemoveProperty(oldProperty);
            }
        }
    }

    private void OnTimerElapsed(object? sender, EventArgs? e)
    {
        if (Timer == null)
            return;

        Timer.Enabled = false;

        IList<Transition> listTransitions;
        lock (Lock)
        {
            listTransitions = [];
            foreach (var pair in Transitions)
                listTransitions.Add(pair.Key);
        }

        foreach (var transition in listTransitions)
            transition.OnTimer();
        Timer.Enabled = true;
    }

    private void OnTransitionCompleted(object? sender, EventArgs? e)
    {
        if (sender is Transition transition)
        {
            transition.TransitionCompletedEvent -= OnTransitionCompleted;
            lock (Lock)
                Transitions.Remove(transition);
        }
    }

    private void RemoveDuplicates(Transition transition)
    {
        foreach (var pair in Transitions)
            RemoveDuplicates(transition, pair.Key);
    }
}