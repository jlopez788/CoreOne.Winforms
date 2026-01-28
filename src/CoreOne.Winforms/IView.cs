using CoreOne.Winforms.Events;

namespace CoreOne.Winforms;

public interface IView
{
    event AsyncRun? AsyncRun;
    event InvokeTaskAsync? AsyncTask;
    event EventHandler<ViewEventArgs>? ChangeView;

    AnchorStyles Anchor { get; set; }
    Point Location { get; set; }
    string Name { get; }
    Size Size { get; set; }
    string Text { get; }

    void Reload(IReadOnlyList<object>? args = null);

    void ShouldDisplay(ref ViewEventArgs e);

    void ViewLoaded();
}