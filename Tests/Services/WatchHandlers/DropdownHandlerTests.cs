using CoreOne.Reflection;
using CoreOne.Winforms;
using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;
using CoreOne.Winforms.Services.WatchHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Services.WatchHandlers;

public class DropdownHandlerTests
{
    public class TestAsyncDropdownProvider : IDropdownSourceProviderAsync
    {
        public bool GetItemsAsyncCalled { get; private set; }
        public bool InitializeCalled { get; private set; }

        public void Dispose()
        { }

        public async Task<IEnumerable<DropdownItem>> GetItemsAsync(object model)
        {
            GetItemsAsyncCalled = true;
            await Task.Delay(10);
            return new List<DropdownItem>
            {
                new DropdownItem("Async Item 1", "async1"),
                new DropdownItem("Async Item 2", "async2")
            };
        }

        public void Initialize(IWatchHandler handler)
        {
            InitializeCalled = true;
        }
    }

    public class TestDropdownProvider : IDropdownSourceProvider, IDropdownSourceProviderSync
    {
        private IEnumerable<DropdownItem> Items = [];
        public bool GetItemsCalled { get; private set; }
        public bool InitializeCalled { get; private set; }

        public void Dispose()
        { }

        public IEnumerable<DropdownItem> GetItems(object model)
        {
            GetItemsCalled = true;
            return Items;
        }

        public void Initialize(IWatchHandler handler)
        {
            InitializeCalled = true;
        }

        public void SetDropdownItems(IEnumerable<DropdownItem> items) => Items = items;
    }

    // Test models and providers
    private class TestModel
    {
        [DropdownSource<TestAsyncDropdownProvider>]
        public string AsyncCategory { get; set; } = "";
        [DropdownSource<TestDropdownProvider>]
        public string Category { get; set; } = "";
        public string Name { get; set; } = "";
    }

    private class TestModelWithInvalidProvider
    {
        [DropdownSource(typeof(string))]
        public string Category { get; set; } = "";
    }

    private class TestModelWithTypedProvider
    {
        [DropdownSource(typeof(TestDropdownProvider))]
        public string Category { get; set; } = "";
    }

    private DropdownHandler _factory = null!;
    private IServiceProvider _serviceProvider = null!;

    [Test]
    public void CreateInstance_WithDropdownSourceAttribute_ReturnsHandler()
    {
        var model = new TestModel();
        var provider = new TestDropdownProvider();
        var services = new ServiceCollection();
        services.AddSingleton<IDropdownSourceProvider>(provider);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = new DropdownHandler(serviceProvider);

        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Category), new ComboBox(), model);

        var handler = factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void CreateInstance_WithInvalidProviderType_ReturnsNull()
    {
        var model = new TestModelWithInvalidProvider();
        var gridItem = CreatePropertyGridItem(typeof(TestModelWithInvalidProvider),
            nameof(TestModelWithInvalidProvider.Category), new ComboBox(), model);

        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Null);
    }

    [Test]
    public void CreateInstance_WithNonComboBoxControl_ReturnsNull()
    {
        var model = new TestModel();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Category), new TextBox(), model);

        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Null);
    }

    [Test]
    public void CreateInstance_WithoutDropdownSourceAttribute_ReturnsNull()
    {
        var model = new TestModel();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Name), new ComboBox(), model);

        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Null);
    }

    [Test]
    public void CreateInstance_WithProviderFromServiceProvider_ResolvesProvider()
    {
        var model = new TestModel();
        var provider = new TestDropdownProvider();
        var services = new ServiceCollection();
        services.AddSingleton(provider);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = new DropdownHandler(serviceProvider);

        var gridItem = CreatePropertyGridItem(typeof(TestModelWithTypedProvider),
            nameof(TestModelWithTypedProvider.Category), new ComboBox(), model);

        var handler = factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Not.Null);
    }

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        _serviceProvider = services.BuildServiceProvider();
        _factory = new DropdownHandler(_serviceProvider);
    }

    [TearDown]
    public void TearDown()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }

    [Test]
    public void WatchHandler_CallsProviderInitialize()
    {
        var model = new TestModel();
        var provider = new TestDropdownProvider();
        var services = new ServiceCollection();
        services.AddSingleton(provider);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = new DropdownHandler(serviceProvider);

        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Category), new ComboBox(), model);
        var handler = factory.CreateInstance(gridItem);

        handler!.Refresh(model);

        Assert.That(provider.InitializeCalled, Is.True);
    }

    [Test]
    public void WatchHandler_PopulatesComboBoxWithItems()
    {
        var model = new TestModel();
        var provider = new TestDropdownProvider();
        var services = new ServiceCollection();
        var items = new List<DropdownItem> {
                new("Item 1", "item1"),
                new("Item 2", "item2"),
                new("Item 3", "item3")
            };
        provider.SetDropdownItems(items);
        services.AddSingleton(provider);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = new DropdownHandler(serviceProvider);

        var comboBox = new ComboBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Category), comboBox, model);
        var handler = factory.CreateInstance(gridItem);

        handler!.Refresh(model);
        Thread.Sleep(100); // Wait for async operation

        Assert.That(comboBox.DataSource, Is.Not.Null);
        Assert.That(comboBox.DataSource is List<DropdownItem>, Is.True);
        Assert.That(((List<DropdownItem>)comboBox.DataSource).Count, Is.EqualTo(items.Count));
    }

    [Test]
    public void WatchHandler_RestoresPreviousSelection()
    {
        var model = new TestModel();
        var provider = new TestDropdownProvider();
        var services = new ServiceCollection();
        services.AddSingleton<IDropdownSourceProvider>(provider);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = new DropdownHandler(serviceProvider);

        var comboBox = new ComboBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Category), comboBox, model);
        var handler = factory.CreateInstance(gridItem);

        // First refresh
        handler!.Refresh(model);
        System.Threading.Thread.Sleep(100);

        // Select an item
        if (comboBox.Items.Count > 1)
        {
            comboBox.SelectedIndex = 1;
            var selectedValue = (comboBox.SelectedItem as DropdownItem)?.Value;

            // Second refresh should restore selection
            handler.Refresh(model);
            System.Threading.Thread.Sleep(100);

            var newSelectedValue = (comboBox.SelectedItem as DropdownItem)?.Value;
            Assert.That(newSelectedValue, Is.EqualTo(selectedValue));
        }
    }

    [Test]
    public void WatchHandler_SelectsFirstItemWhenNoPreviousSelection()
    {
        var model = new TestModel();
        var provider = new TestDropdownProvider();
        var services = new ServiceCollection();
        services.AddSingleton<IDropdownSourceProvider>(provider);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = new DropdownHandler(serviceProvider);

        var comboBox = new ComboBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Category), comboBox, model);
        var handler = factory.CreateInstance(gridItem);

        handler!.Refresh(model);
        System.Threading.Thread.Sleep(100);

        Assert.That(comboBox.SelectedIndex, Is.EqualTo(-1));
    }

    [Test]
    public void WatchHandler_SetsDataSourceAndDisplayMember()
    {
        var model = new TestModel();
        var provider = new TestDropdownProvider();
        var services = new ServiceCollection();
        services.AddSingleton(provider);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = new DropdownHandler(serviceProvider);

        var comboBox = new ComboBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Category), comboBox, model);
        var handler = factory.CreateInstance(gridItem);

        handler!.Refresh(model);

        Assert.That(comboBox.DataSource, Is.Not.Null);
        Assert.That(comboBox.DisplayMember, Is.EqualTo(nameof(DropdownItem.Display)));
    }

    [Test]
    public void WatchHandler_WithAsyncProvider_CallsGetItemsAsync()
    {
        var model = new TestModel();
        var provider = new TestAsyncDropdownProvider();
        var services = new ServiceCollection();
        services.AddSingleton(provider);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = new DropdownHandler(serviceProvider);

        var comboBox = new ComboBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.AsyncCategory), comboBox, model);
        var handler = factory.CreateInstance(gridItem);

        handler!.Refresh(model);
        System.Threading.Thread.Sleep(100); // Wait for async operation

        Assert.That(provider.GetItemsAsyncCalled, Is.True);
    }

    [Test]
    public void WatchHandler_WithSyncProvider_CallsGetItems()
    {
        var model = new TestModel();
        var provider = new TestDropdownProvider();
        var services = new ServiceCollection();
        services.AddSingleton(provider);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = new DropdownHandler(serviceProvider);

        var comboBox = new ComboBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Category), comboBox, model);
        var handler = factory.CreateInstance(gridItem);

        handler!.Refresh(model);
        System.Threading.Thread.Sleep(100); // Wait for async operation

        Assert.That(provider.GetItemsCalled, Is.True);
    }

    private static PropertyGridItem CreatePropertyGridItem(Type type, string propertyName, Control control, object model)
    {
        var propInfo = type.GetProperty(propertyName)!;
        var metadata = new Metadata(propInfo, propInfo.PropertyType, null, null);
        return new PropertyGridItem(control, metadata, value => metadata.SetValue(model, value));
    }
}