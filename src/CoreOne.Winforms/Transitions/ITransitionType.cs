namespace CoreOne.Winforms.Transitions;

public interface ITransitionType
{
    void OnTimer(int ts, out double percentage, out bool bCompleted);
}