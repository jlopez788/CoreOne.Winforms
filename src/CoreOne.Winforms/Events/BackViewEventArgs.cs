namespace CoreOne.Winforms.Events;

public record BackViewEventArgs : ViewEventArgs
{
    public BackViewEventArgs() : base(ViewActionType.GoBackView)
    {
        AddToHistory = false;
    }
}