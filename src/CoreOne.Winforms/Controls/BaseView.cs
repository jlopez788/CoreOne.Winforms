using CoreOne.Winforms.Events;
using System.ComponentModel;

namespace CoreOne.Winforms.Controls;

public class BaseView : Control, IView
{
    public event AsyncRun? AsyncRun;
    public event InvokeTaskAsync? AsyncTask;
    public event EventHandler<ViewEventArgs>? ChangeView;

    public new IContainer Container { get; }

    public BaseView()
    {
        Container = new Container();
        InitializeComponent();
    }

    public void ApplyTheme(Theme theme) => OnApplyTheme(theme);

    public void Reload(IReadOnlyList<object>? args = null) => OnReload(args);

    public void ShouldDisplay(ref ViewEventArgs e) => OnDisplay(ref e);

    public void ViewLoaded() => OnViewLoaded();

    protected static bool GetIndex<T>(IReadOnlyList<object> args, int idx, Action<T> callback)
    {
        bool invoked = false;
        if (args?.Count > 0 && idx < args.Count && args[idx] is T model)
        {
            invoked = true;
            callback?.Invoke(model);
        }

        return invoked;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Container?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected virtual void OnApplyTheme(Theme theme)
    {
    }

    protected virtual void OnAsyncRun(Action action) => AsyncRun?.Invoke(action);

    protected virtual void OnAsyncTask(InvokeTask task) => AsyncTask?.Invoke(task);

    protected void OnChangeView(string name, params object[] args) => OnChangeView(new ViewEventArgs(name, args: args));

    protected virtual void OnChangeView(ViewEventArgs e) => ChangeView?.Invoke(this, e);

    protected virtual void OnDisplay(ref ViewEventArgs e)
    {
    }

    protected virtual void OnReload(IReadOnlyList<object>? args = null)
    {
    }

    protected virtual void OnViewLoaded()
    {
    }

    protected virtual void RegisterComponent(params IComponent[] components)
    {
        foreach (var c in components)
        {
            if (c != null)
            {
                Container.Add(c);
            }
        }
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        //
        // BaseView
        //
        BackColor = Color.White;
        Font = new System.Drawing.Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
        Name = "BaseView";
        ResumeLayout(false);
    }
}