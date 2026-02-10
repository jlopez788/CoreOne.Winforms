using CoreOne.Winforms.Services;

namespace Tests.Services;

public class FormHostedServiceTests
{
    private class TestFormHostedService : FormHostedService
    {
        public bool InitializeCalled { get; private set; }
        public bool StoppingServiceCalled { get; private set; }
        public CancellationToken InitializeToken { get; private set; }
        public CancellationToken StoppingToken { get; private set; }

        public override ValueTask Initialize(CancellationToken cancellationToken)
        {
            InitializeCalled = true;
            InitializeToken = cancellationToken;
            return ValueTask.CompletedTask;
        }

        public override ValueTask StoppingService(CancellationToken cancellationToken)
        {
            StoppingServiceCalled = true;
            StoppingToken = cancellationToken;
            return ValueTask.CompletedTask;
        }
    }

    [Test]
    public async Task Initialize_DefaultImplementation_CompletesSuccessfully()
    {
        var service = new FormHostedService();

        var result = service.Initialize(CancellationToken.None);

        Assert.That(result.IsCompletedSuccessfully, Is.True);
        await result;
    }

    [Test]
    public async Task StoppingService_DefaultImplementation_CompletesSuccessfully()
    {
        var service = new FormHostedService();

        var result = service.StoppingService(CancellationToken.None);

        Assert.That(result.IsCompletedSuccessfully, Is.True);
        await result;
    }

    [Test]
    public async Task Initialize_OverriddenImplementation_IsCalled()
    {
        var service = new TestFormHostedService();
        var cts = new CancellationTokenSource();

        await service.Initialize(cts.Token);

        Assert.That(service.InitializeCalled, Is.True);
        Assert.That(service.InitializeToken, Is.EqualTo(cts.Token));
    }

    [Test]
    public async Task StoppingService_OverriddenImplementation_IsCalled()
    {
        var service = new TestFormHostedService();
        var cts = new CancellationTokenSource();

        await service.StoppingService(cts.Token);

        Assert.That(service.StoppingServiceCalled, Is.True);
        Assert.That(service.StoppingToken, Is.EqualTo(cts.Token));
    }

    [Test]
    public async Task Initialize_WithCancelledToken_CompletesSuccessfully()
    {
        var service = new FormHostedService();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = service.Initialize(cts.Token);

        Assert.That(result.IsCompletedSuccessfully, Is.True);
        await result;
    }

    [Test]
    public async Task StoppingService_WithCancelledToken_CompletesSuccessfully()
    {
        var service = new FormHostedService();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = service.StoppingService(cts.Token);

        Assert.That(result.IsCompletedSuccessfully, Is.True);
        await result;
    }
}
