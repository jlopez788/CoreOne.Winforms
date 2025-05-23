using CoreOne.Winforms.Native;
using CoreOne.Winforms.Services;
using Microsoft.Extensions.DependencyInjection;

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

    public static IResult Run<T>(IServiceProvider? serviceProvider, bool sinlgeInstance = false) where T : Form
    {
        Mutex? mutex = null;
        var result = Result.Ok;
        try
        {
            if (serviceProvider != null)
                Services = serviceProvider;
            if (sinlgeInstance)
            {
                var createForm = CreateFormMutex<T>(out mutex);
                if (createForm)
                {
                    result = RunForm<T>(serviceProvider, true);
                }
                else
                {
                    ShowFirstInstance();
                }
            }
            else
            {
                result = RunForm<T>(serviceProvider);
            }
        }
        catch (Exception ex)
        {
            result = new Result(ResultType.Exception, ex.Message);
        }
        finally
        {
            mutex?.ReleaseMutex();
        }

        return result;
    }

    public static async ValueTask<IResult> RunAsync<T>(IServiceProvider serviceProvider, bool sinlgeInstance = false) where T : Form
    {
        Mutex? mutex = null;
        var result = Result.Ok;
        try
        {
            Services = serviceProvider;

            var host = serviceProvider.GetService<FormContainerHostService>();
            try
            {
                if (host != null)
                    await host.Initialize();
                if (sinlgeInstance)
                {
                    var createForm = CreateFormMutex<T>(out mutex);
                    if (createForm)
                    {
                        result = RunForm<T>(serviceProvider, true);
                    }
                    else
                    {
                        ShowFirstInstance();
                    }
                }
                else
                {
                    result = RunForm<T>(serviceProvider);
                }
            }
            finally
            {
                if (host != null)
                {
                    await host.Stop();
                    await host.DisposeAsync();
                }
            }
        }
        catch (Exception ex)
        {
            result = new Result(ResultType.Exception, ex.Message);
        }
        finally
        {
            mutex?.ReleaseMutex();
        }

        return result;
    }

    private static bool CreateFormMutex<T>(out Mutex mutex)
    {
        var type = typeof(T);
        AssemblyGuid = Utility.Crc32(type.FullName).Model ?? "";
        mutex = new Mutex(true, Name, out bool createForm);
        return createForm;
    }

    private static IResult RunForm<T>(IServiceProvider? serviceProvider, bool attachHandler = false) where T : Form
    {
        var result = Result.Ok;
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var type = typeof(T);
            var instance = serviceProvider?.GetService(type);
            instance ??= serviceProvider.GetTargetCreator()
                .CreateInstance(type);
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
                Application.Run(form);
            }
        }
        catch (Exception ex)
        {
            result = Result.FromException(ex);
        }

        return result;
    }

    private static void ShowFirstInstance() => WindowsApi.PostMessage(WindowsApi.HWND_BROADCAST, WM_SHOWFIRSTINSTANCE, IntPtr.Zero, IntPtr.Zero);
}