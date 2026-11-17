using Microsoft.Extensions.Hosting;

namespace CoreOne.Winforms.Services;

public class FormContainerHostService(IEnumerable<IHostedService> hostedServices) : IAsyncDisposable
{
    private readonly AToken Token = AToken.Create();
    private bool _stopped;

    public async ValueTask DisposeAsync()
    {
        if (!_stopped)
            await Stop();

        await Token.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    public ValueTask Initialize() => ExecuteHostedServices((p, ct) => p.StartAsync(ct), Token);

    public async ValueTask Stop()
    {
        _stopped = true;
        await ExecuteHostedServices((p, ct) => p.StopAsync(ct), Token);
    }

    private async ValueTask ExecuteHostedServices(Func<IHostedService, CancellationToken, Task> callback, CancellationToken cancellationToken)
    {
        if (hostedServices.Any())
        {
            var tasks = hostedServices.SelectArray(p => callback(p, cancellationToken));
            await Task.WhenAll(tasks);
        }
    }
}