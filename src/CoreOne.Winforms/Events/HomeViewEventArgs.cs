namespace CoreOne.Winforms.Events;

public record HomeViewEventArgs : ViewEventArgs
{
    public HomeViewEventArgs(bool animate = true) : base(ViewActionType.GoHomeView)
    {
        Animate = animate;
    }
}