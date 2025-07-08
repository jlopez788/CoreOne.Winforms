namespace CoreOne.Winforms.Services;

internal class FormContainerHostService(IEnumerable<IHostedFormService> services)
{
    private readonly AToken Token = AToken.Create();

    public async ValueTask DisposeAsync()
    {
        await Execute((p, ct) => {
            if (p is IAsyncDisposable adisposable)
                return adisposable.DisposeAsync();
            else if (p is IDisposable disposable)
                disposable.Dispose();
            return ValueTask.CompletedTask;
        }, CancellationToken.None);
    }

    public ValueTask Initialize() => Execute((p, ct) => p.Initialize(ct), Token);

    public ValueTask Stop() => Execute((p, ct) => p.StoppingService(ct), Token);

    private async ValueTask Execute(Func<IHostedFormService, CancellationToken, ValueTask> callback, CancellationToken cancellationToken)
    {
        if (services.Any())
        {
            var tasks = services.SelectArray(p => callback(p, cancellationToken).AsTask());
            await Task.WhenAll(tasks);
        }
    }
}