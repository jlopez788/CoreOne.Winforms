using CoreOne.Reactive;
using CoreOne.Winforms.Events;
using CoreOne.Winforms.Forms;
using CoreOne.Winforms.Services;
using CoreOne.Winforms.Transitions.Animations;

namespace CoreOne.Winforms.Controls;

public class AnimatedPanel : Control
{
    private record CurrentContext(IView View, ViewEventArgs Args);
    public event EventHandler? AnimationComplete;
    private readonly Stack<History> History;
    private readonly Point Origin = new(0, 0);
    private readonly List<Func<IView, ViewEventArgs, bool>> Rules;
    private readonly Subject<(History history, bool isEmpty)> Stream = new();
    private readonly SToken Token = SToken.Create();
    private CurrentContext? Current;
    public IView? CurrentView => Current?.View;
    public bool IsAnimating { get; private set; }
    public TypeViewManager? ViewManager { get; set; }
    protected string? Home { get; private set; }
    private Point LeftEdge => new(Width, 0);

    public AnimatedPanel()
    {
        History = new Stack<History>(100);
        Rules = [
            (v,_)=> v is null,
            sameName,
            shouldDisplay
        ];

        bool sameName(IView view, ViewEventArgs args)
        {
            var sameView = CurrentView?.Name.Matches(view.Name) ?? false;
            var sameArgs = sameView && (Current?.Args.Equals(args) ?? false);
            return !(!sameView || (sameView && !sameArgs));
        }
        bool shouldDisplay(IView view, ViewEventArgs args)
        {
            view.ShouldDisplay(ref args);
            return args.Cancel;
        }
    }

    public void ChangeView(ViewEventArgs e, bool swipeleft = true) => this.CrossThread(() => OnChangeView(e, swipeleft));

    public void GoBack() => ChangeView(new BackViewEventArgs(), false);

    public void GoHome(bool animate = true) => ChangeView(new HomeViewEventArgs(animate));

    public bool HasHistory() => History.Count > 0;

    public void SetHomePage(string home) => Home = home;

    public void SubscribeToHistory(Action<(History history, bool isEmpty)> callback, CancellationToken cancellationToken)
    {
        Stream.Subscribe(callback, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        Token.Dispose();
        base.Dispose(disposing);
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        ChangeView(new ViewEventArgs(Home ?? string.Empty, false, false));
    }

    private void OnAnimationComplete()
    {
        CurrentView?.ViewLoaded();
        AnimationComplete?.Invoke(this, EventArgs.Empty);
    }

    private void OnChangeView(ViewEventArgs args, bool swipeleft = true)
    {
        if (args.ViewActionType == ViewActionType.GoHomeView)
        {
            if (string.IsNullOrEmpty(Home))
            {
                return;
            }
        }
        else if (args.ViewActionType == ViewActionType.GoBackView)
        {
            if (!HasHistory())
            {
                return;
            }
            var history = History.Pop();
            swipeleft = false;
            args = args with {
                Name = history.ViewName,
                Args = history.Args
            };

            Stream.OnNext((history, History.Count == 0));
        }
        var view = ResolveView(args);
        if (view == null || Rules.Any(rule => rule(view, args)))
        {
            return;
        }

        Current = new CurrentContext(view, args);
        if (args.AddToHistory)
        {
            var history = new History(args.Name, args.Args);
            History.Push(history);
            Stream.OnNext((history, History.Count == 0));
        }
        view.Anchor = (AnchorStyles)0xf;
        view.Size = Size;
        view.Reload(args.Args);

        void AnimationComplete(object? sender, EventArgs _)
        {
            IsAnimating = false;
            if (Controls.Count == 2)
            {
                Controls[0].Dispose();
            }
            OnAnimationComplete();
        }

        if (args.Animate)
        {
            var transition = Animation.Linear(400).ToTransition();
            Point
                starting = swipeleft ? LeftEdge : Origin,
                ending = swipeleft ? Origin : LeftEdge;
            transition.TransitionCompletedEvent += AnimationComplete;
            view.Location = starting;
            Controls.Add((Control)view);
            if (Controls.Count > 1)
            {
                if (swipeleft)
                {
                    transition.Add(Controls[0], "Location", starting);
                    transition.Add(Controls[1], "Location", ending);
                }
                else
                {
                    transition.Add(Controls[0], "Location", LeftEdge);
                }
            }
            else
            {
                transition.Add(Controls[0], "Location", ending);
            }
            transition.Run(Token);
        }
        else
        {
            Controls.Add((Control)view);
            AnimationComplete(this, EventArgs.Empty);
        }
    }

    private IView? ResolveView(ViewEventArgs eventArgs)
    {
        var view = ViewManager?.Resolve(eventArgs);
        if (view != null)
        {
            var parent = FindForm();
            if (parent is OneForm form)
            {
                view.AsyncRun += form.RunActionAsync;
                view.AsyncTask += form.RunAsyncTask;
                view.ChangeView += (s, e) => form.ChangeView(e);
            }
        }
        return view;
    }
}