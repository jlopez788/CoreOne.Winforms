namespace CoreOne.Winforms.Services;

public class FormHostedService : IHostedFormService
{
    public virtual ValueTask Initialize(CancellationToken cancellationToken) => ValueTask.CompletedTask;

    public virtual ValueTask StoppingService(CancellationToken cancellationToken) => ValueTask.CompletedTask;
}