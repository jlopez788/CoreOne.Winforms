using CoreOne.Winforms.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CoreOne.Winforms.Tests.Extensions;

[TestFixture]
public class ServiceProviderExtensionsTests
{
    [Test]
    public void AddFormServices_RegistersAllRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddFormServices();
        Assert.Multiple(() => {
            // Just verify services are registered
            Assert.That(services.Any(s => s.ServiceType == typeof(IPropertyGridItemFactory)), Is.True);
            Assert.That(services.Any(s => s.ServiceType == typeof(IGridLayoutManager)), Is.True);
            Assert.That(services.Any(s => s.ServiceType == typeof(IModelBinder)), Is.True);
        });
    }

    [Test]
    public void AddFormServices_RegistersControlFactories()
    {
        var services = new ServiceCollection();
        services.AddFormServices();

        var factories = services.Where(s => s.ServiceType == typeof(IControlFactory));
        Assert.That(factories, Is.Not.Null);
        Assert.That(factories.Any(), Is.True);
    }

    [Test]
    public void AddFormServices_RegistersWatchHandlers()
    {
        var services = new ServiceCollection();
        services.AddFormServices();

        var handlers = services.Where(s => s.ServiceType == typeof(IWatchFactory));
        Assert.That(handlers, Is.Not.Null);
        Assert.That(handlers.Any(), Is.True);
    }
}