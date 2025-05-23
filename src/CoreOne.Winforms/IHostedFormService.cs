namespace CoreOne.Winforms;

public interface IHostedFormService
{
    ValueTask Initialize(CancellationToken cancellationToken);

    ValueTask StoppingService(CancellationToken cancellationToken);
}