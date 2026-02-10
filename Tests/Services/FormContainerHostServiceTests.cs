using CoreOne.Winforms;
using CoreOne.Winforms.Services;
using Moq;

namespace Tests.Services;

public class FormContainerHostServiceTests
{
    private class TestHostedService : IHostedFormService
    {
        public bool InitializeCalled { get; private set; }
        public bool StoppingCalled { get; private set; }
        public int InitializeCallCount { get; private set; }
        public int StoppingCallCount { get; private set; }

        public ValueTask Initialize(CancellationToken cancellationToken)
        {
            InitializeCalled = true;
            InitializeCallCount++;
            return ValueTask.CompletedTask;
        }

        public ValueTask StoppingService(CancellationToken cancellationToken)
        {
            StoppingCalled = true;
            StoppingCallCount++;
            return ValueTask.CompletedTask;
        }
    }

    private class AsyncDisposableService : IHostedFormService, IAsyncDisposable
    {
        public bool DisposeCalled { get; private set; }

        public ValueTask DisposeAsync()
        {
            DisposeCalled = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask Initialize(CancellationToken cancellationToken) => ValueTask.CompletedTask;

        public ValueTask StoppingService(CancellationToken cancellationToken) => ValueTask.CompletedTask;
    }

    private class SyncDisposableService : IHostedFormService, IDisposable
    {
        public bool DisposeCalled { get; private set; }

        public void Dispose()
        {
            DisposeCalled = true;
        }

        public ValueTask Initialize(CancellationToken cancellationToken) => ValueTask.CompletedTask;

        public ValueTask StoppingService(CancellationToken cancellationToken) => ValueTask.CompletedTask;
    }

    [Test]
    public async Task Initialize_WithNoServices_CompletesSuccessfully()
    {
        var services = Array.Empty<IHostedFormService>();
        var container = new FormContainerHostService(services);

        await container.Initialize();

        Assert.Pass("Initialize completed without throwing");
    }

    [Test]
    public async Task Initialize_WithSingleService_CallsInitialize()
    {
        var service = new TestHostedService();
        var container = new FormContainerHostService(new[] { service });

        await container.Initialize();

        Assert.That(service.InitializeCalled, Is.True);
        Assert.That(service.InitializeCallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Initialize_WithMultipleServices_CallsAllInitializes()
    {
        var service1 = new TestHostedService();
        var service2 = new TestHostedService();
        var service3 = new TestHostedService();
        var container = new FormContainerHostService(new IHostedFormService[] { service1, service2, service3 });

        await container.Initialize();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(service1.InitializeCalled, Is.True);
            Assert.That(service2.InitializeCalled, Is.True);
            Assert.That(service3.InitializeCalled, Is.True);
        }
    }

    [Test]
    public async Task Stop_WithNoServices_CompletesSuccessfully()
    {
        var services = Array.Empty<IHostedFormService>();
        var container = new FormContainerHostService(services);

        await container.Stop();

        Assert.Pass("Stop completed without throwing");
    }

    [Test]
    public async Task Stop_WithSingleService_CallsStoppingService()
    {
        var service = new TestHostedService();
        var container = new FormContainerHostService(new[] { service });

        await container.Stop();

        Assert.That(service.StoppingCalled, Is.True);
        Assert.That(service.StoppingCallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Stop_WithMultipleServices_CallsAllStoppingServices()
    {
        var service1 = new TestHostedService();
        var service2 = new TestHostedService();
        var service3 = new TestHostedService();
        var container = new FormContainerHostService(new IHostedFormService[] { service1, service2, service3 });

        await container.Stop();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(service1.StoppingCalled, Is.True);
            Assert.That(service2.StoppingCalled, Is.True);
            Assert.That(service3.StoppingCalled, Is.True);
        }
    }

    [Test]
    public async Task DisposeAsync_WithAsyncDisposableService_CallsDisposeAsync()
    {
        var service = new AsyncDisposableService();
        var container = new FormContainerHostService(new IHostedFormService[] { service });

        await container.DisposeAsync();

        Assert.That(service.DisposeCalled, Is.True);
    }

    [Test]
    public async Task DisposeAsync_WithSyncDisposableService_CallsDispose()
    {
        var service = new SyncDisposableService();
        var container = new FormContainerHostService(new IHostedFormService[] { service });

        await container.DisposeAsync();

        Assert.That(service.DisposeCalled, Is.True);
    }

    [Test]
    public async Task DisposeAsync_WithMixedDisposableServices_CallsBothDisposeMethods()
    {
        var asyncService = new AsyncDisposableService();
        var syncService = new SyncDisposableService();
        var container = new FormContainerHostService(new IHostedFormService[] { asyncService, syncService });

        await container.DisposeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(asyncService.DisposeCalled, Is.True);
            Assert.That(syncService.DisposeCalled, Is.True);
        }
    }

    [Test]
    public async Task DisposeAsync_WithNonDisposableService_CompletesSuccessfully()
    {
        var service = new TestHostedService();
        var container = new FormContainerHostService(new[] { service });

        await container.DisposeAsync();

        Assert.Pass("DisposeAsync completed without throwing");
    }

    [Test]
    public async Task Initialize_WithMockedService_PassesCancellationToken()
    {
        var mockService = new Mock<IHostedFormService>();
        mockService.Setup(s => s.Initialize(It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var container = new FormContainerHostService(new[] { mockService.Object });

        await container.Initialize();

        mockService.Verify(s => s.Initialize(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Stop_WithMockedService_PassesCancellationToken()
    {
        var mockService = new Mock<IHostedFormService>();
        mockService.Setup(s => s.StoppingService(It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var container = new FormContainerHostService(new[] { mockService.Object });

        await container.Stop();

        mockService.Verify(s => s.StoppingService(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Initialize_ThenStop_BothMethodsCalled()
    {
        var service = new TestHostedService();
        var container = new FormContainerHostService(new[] { service });

        await container.Initialize();
        await container.Stop();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(service.InitializeCalled, Is.True);
            Assert.That(service.StoppingCalled, Is.True);
        }
    }
}
