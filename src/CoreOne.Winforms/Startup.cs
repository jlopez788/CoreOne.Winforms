using CoreOne.Winforms.Native;
using CoreOne.Winforms.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreOne.Winforms;

public static class Startup
{
    public static IServiceProvider Services { get; private set; } = default!;
    internal static string AssemblyGuid { get; private set; } = default!;
    internal static string Name => $"Local\\{WinMessage}";
    internal static string WinMessage => $"WM_SHOWFIRSTINSTANCE|{AssemblyGuid}";
    internal static int WM_SHOWFIRSTINSTANCE { get; set; }

    public static TService Get<TService>() where TService : notnull => Services is not null ?
            Services.Resolve<TService>() :
            throw new NotImplementedException($"{nameof(Services)} property has not been initialized");

    public static IResult Run<T>(bool sinlgeInstance = false) where T : Form => Run<T>(null, sinlgeInstance);

    public static IResult Run<T>(IServiceProvider? services, bool sinlgeInstance = false) where T : Form
    {
        Mutex? mutex = null;
        var ownsMutex = false;
        var appContext = new ApplicationContext();
        var task = new TaskCompletionSource<IResult>();
        var host = services?.GetService<FormContainerHostService>();
        var logger = services?.GetService<ILogger<T>>();

        if (SynchronizationContext.Current is not WindowsFormsSynchronizationContext)
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

        ApplicationRunAsync();
        Application.Run(appContext);

        if (ownsMutex)
            mutex?.ReleaseMutex();
        mutex?.Dispose();

        return Task.Factory.StartNew(async () => {
            if (host != null)
            {
                await host.Stop();
                await host.DisposeAsync();
            }

            return await task.Task;
        }).Unwrap().GetAwaiter().GetResult();

        async void ApplicationRunAsync()
        {
            try
            {
                if (host != null)
                    await host.Initialize();

                if (sinlgeInstance)
                {
                    ownsMutex = CreateFormMutex<T>(out mutex);
                    if (ownsMutex)
                    {
                        AssignFormToContext();
                    }
                    else
                    {
                        ShowFirstInstance();
                        appContext.ExitThread();
                    }
                }
                else
                {
                    AssignFormToContext();
                }

                task.TrySetResult(Result.Ok);
            }
            catch (Exception ex)
            {
                logger?.LogEntryX(ex, "Startup.Run<{Type}> failed: {Message}", typeof(T).Name, ex.Message);
                task.TrySetResult(new Result(ResultType.Exception, ex.Message));
                appContext.ExitThread();
            }
        }
        void AssignFormToContext()
        {
            var form = GetFormInstance<T>(services, sinlgeInstance);
            if (form is not null)
            {
                appContext.MainForm = form;
                form.Show();
            }
            else
            {
                appContext.ExitThread();
            }
        }
    }

    private static bool CreateFormMutex<T>(out Mutex mutex)
    {
        var type = typeof(T);
        AssemblyGuid = Utility.Crc32(type.FullName).Model ?? "";
        if (WM_SHOWFIRSTINSTANCE == 0)
            WM_SHOWFIRSTINSTANCE = WindowsApi.RegisterWindowMessage(WinMessage);
        mutex = new Mutex(true, Name, out bool createForm);
        return createForm;
    }

    private static T? GetFormInstance<T>(IServiceProvider? services, bool attachHandler = false) where T : Form
    {
        var type = typeof(T);
        var instance = services?.GetService(type);
        instance ??= services?.GetTargetCreator()
            .CreateInstance(type);
        instance ??= TypeUtility.GetInstance(type);
        if (instance is T form)
        {
            if (attachHandler)
            {
                FormClosingEventHandler disposed = default!;
                var handler = new EventHandler((s, e) => Application.AddMessageFilter(new ShowFormMessageFilter(form.Handle)));
                disposed = new FormClosingEventHandler((s, e) => {
                    form.HandleCreated -= handler;
                    form.FormClosing -= disposed;
                });
                form.HandleCreated += handler;
                form.FormClosing += disposed;
            }

            return form;
        }

        return null;
    }

    private static void ShowFirstInstance() => WindowsApi.PostMessage(WindowsApi.HWND_BROADCAST, WM_SHOWFIRSTINSTANCE, IntPtr.Zero, IntPtr.Zero);
}