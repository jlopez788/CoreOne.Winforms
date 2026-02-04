using CoreOne.Extensions;
using CoreOne.Reactive;
using CoreOne.Reflection;
using CoreOne.Winforms;
using CoreOne.Winforms.Events;
using CoreOne.Winforms.Models;
using CoreOne.Winforms.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Reflection;

namespace Tests.Services;

public class ModelBinderTests
{
    private class TestModel
    {
        public int Age { get; set; }
        public string Name { get; set; } = "";
    }

    private class TestModelWithIgnored
    {
        public string Name { get; set; } = "";

        [CoreOne.Winforms.Attributes.Ignore]
        public string Secret { get; set; } = "";
    }

    private class TestNumericControlFactory : IControlFactory
    {
        public int Priority => 0;

        public bool CanHandle(Metadata property) => property.FPType == typeof(int);

        public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
        {
            var numericUpDown = new NumericUpDown();
            numericUpDown.Name = property.Name;
            return new ControlContext(numericUpDown,
                value => numericUpDown.Value = value is int i ? i : 0,
                () => numericUpDown.ValueChanged += (s, e) => onValueChanged((int)numericUpDown.Value));
        }
    }

    // Test implementations
    private class TestStringControlFactory : IControlFactory
    {
        public int Priority => 0;

        public bool CanHandle(Metadata property) => property.FPType == typeof(string);

        public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
        {
            var textBox = new TextBox();
            textBox.Name = property.Name;

            return new ControlContext(textBox,
                value => textBox.Text = value?.ToString() ?? "",
                () => textBox.TextChanged += (s, e) => onValueChanged(textBox.Text));
        }
    }

    private ModelBinder _binder = null!;
    private Mock<IGridLayoutManager> _mockLayoutManager = null!;
    private Mock<IRefreshManager> _mockRefreshManager = null!;
    private IServiceProvider _serviceProvider = null!;

    [Test]
    public void BindModel_CallsLayoutManagerCorrectly()
    {
        var model = new TestModel { Name = "Test" };
        var container = new Panel();

        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control?, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 150));

        _binder.BindModel(container, model);

        _mockLayoutManager.Verify(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control?, GridColumnSpan)>>()), Times.Once);
        _mockLayoutManager.Verify(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()), Times.Once);
    }

    [Test]
    public void BindModel_FiltersIgnoredProperties()
    {
        var model = new TestModelWithIgnored { Name = "Test", Secret = "Hidden" };
        var container = new Panel();

        var capturedSpans = new List<(Control?, GridColumnSpan)>();
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control?, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control?, GridColumnSpan)>>(spans => capturedSpans.AddRange(spans))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(container, model);

        // Should only bind Name property (Secret is ignored)
        Assert.That(capturedSpans.Count, Is.EqualTo(1));
    }

    [Test]
    public void BindModel_MultipleTimes_UnbindsPrevious()
    {
        var model1 = new TestModel { Name = "First" };
        var model2 = new TestModel { Name = "Second" };
        var container = new Panel();

        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control?, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(container, model1);
        _binder.BindModel(container, model2);

        var boundModel = _binder.GetBoundModel();
        Assert.That(boundModel, Is.SameAs(model2));
    }

    [Test]
    public void BindModel_RegistersWatchHandlersWithRefreshManager()
    {
        var mockWatchFactory = new Mock<IWatchFactory>();
        var mockWatchHandler = new Mock<IWatchHandler>();

        mockWatchFactory.Setup(f => f.Priority).Returns(100);
        mockWatchFactory.Setup(f => f.CreateInstance(It.IsAny<PropertyGridItem>()))
            .Returns(mockWatchHandler.Object);

        var services = new ServiceCollection();
        services.AddSingleton<IControlFactory>(new TestStringControlFactory());
        services.AddSingleton<IWatchFactory>(mockWatchFactory.Object);

        using var serviceProvider = services.BuildServiceProvider();
        using var binder = new ModelBinder(serviceProvider, _mockRefreshManager.Object, _mockLayoutManager.Object);

        var model = new TestModel { Name = "Test" };
        var container = new Panel();

        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control?, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        binder.BindModel(container, model);

        _mockRefreshManager.Verify(m => m.RegisterContext(mockWatchHandler.Object, model), Times.AtLeastOnce);
    }

    [Test]
    public void BindModel_WithNullContainer_ThrowsArgumentNullException()
    {
        var model = new TestModel();

        Assert.Throws<ArgumentNullException>(() => _binder.BindModel(null!, model));
    }

    [Test]
    public void BindModel_WithNullModel_ThrowsArgumentNullException()
    {
        var container = new Panel();

        Assert.Throws<ArgumentNullException>(() => _binder.BindModel(container, null!));
    }

    [Test]
    public void BindModel_WithValidModel_BindsSuccessfully()
    {
        var model = new TestModel { Name = "Test", Age = 25 };
        var container = new Panel();

        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control?, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        var size = _binder.BindModel(container, model);

        Assert.That(size.Height, Is.EqualTo(100));
        Assert.That(container.Controls.Count, Is.EqualTo(1));
    }

    [Test]
    public void Dispose_DisposesPropertyChangedSubject()
    {
        var binder = new ModelBinder(_serviceProvider, _mockRefreshManager.Object, _mockLayoutManager.Object);

        Assert.DoesNotThrow(() => binder.Dispose());
    }

    [Test]
    public void GetBoundModel_ReturnsCurrentModel()
    {
        var model = new TestModel { Name = "Test" };
        var container = new Panel();

        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control?, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(container, model);
        var boundModel = _binder.GetBoundModel();

        Assert.That(boundModel, Is.SameAs(model));
    }

    [Test]
    public void GetBoundModel_ReturnsNullWhenNotBound()
    {
        var boundModel = _binder.GetBoundModel();

        Assert.That(boundModel, Is.Null);
    }

    [Test]
    public void PropertyChanged_FiresWhenValueChanges()
    {
        ModelPropertyChanged? capturedArgs = null;
        var model = new TestModel { Name = "Initial" };
        var container = new Panel();
        var binder = new ModelBinder(_serviceProvider, _mockRefreshManager.Object, new GridLayoutManager());

        // Subscribe by wrapping in Observer.Create
        var subscription = binder.PropertyChanged.Subscribe(Observer.Create<ModelPropertyChanged>(args => {
            capturedArgs = args;
        }));

        binder.BindModel(container, model);

        // Simulate property change through control
        var layoutPanel = container.Controls[0] as Panel;
        if (layoutPanel != null)
        {
            // Find the textbox and change its value
            var textBox = FindControlOfType<TextBox>(binder, nameof(TestModel.Name));
            if (textBox != null)
            {
                textBox.Text = "Updated";
            }
        }

        System.Threading.Thread.Sleep(50); // Allow async operations

        Assert.That(capturedArgs, Is.Not.Null);
        Assert.That(capturedArgs!.NewValue, Is.EqualTo("Updated"));

        subscription.Dispose();
    }

    [SetUp]
    public void Setup()
    {
        _mockRefreshManager = new Mock<IRefreshManager>();
        _mockLayoutManager = new Mock<IGridLayoutManager>();

        var services = new ServiceCollection();

        // Register control factories
        services.AddSingleton<IControlFactory>(new TestStringControlFactory());
        services.AddSingleton<IControlFactory>(new TestNumericControlFactory());

        // Register watch factories (empty for basic tests)
        services.AddSingleton<IEnumerable<IWatchFactory>>(new List<IWatchFactory>());

        _serviceProvider = services.BuildServiceProvider();

        _binder = new ModelBinder(_serviceProvider, _mockRefreshManager.Object, _mockLayoutManager.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _binder?.Dispose();
        (_serviceProvider as IDisposable)?.Dispose();
    }

    [Test]
    public void UnbindModel_CallsRefreshManagerClear()
    {
        var model = new TestModel { Name = "Test" };
        var container = new Panel();

        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control?, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(container, model);
        _binder.UnbindModel();

        _mockRefreshManager.Verify(m => m.Clear(), Times.Exactly(2));
    }

    [Test]
    public void UnbindModel_ClearsBinding()
    {
        var model = new TestModel { Name = "Test" };
        var container = new Panel();

        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control?, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(container, model);
        _binder.UnbindModel();

        var boundModel = _binder.GetBoundModel();
        Assert.That(boundModel, Is.Null);
    }

    private static T? FindControlOfType<T>(ModelBinder modelBinder, string name) where T : Control
    {
        var meta = MetaType.GetMetadata(typeof(ModelBinder), "GridItems", BindingFlags.Instance | BindingFlags.NonPublic);
        if (meta.GetValue(modelBinder) is List<PropertyGridItem> entries)
        {
            var entry = entries.FirstOrDefault(p => p.Property.Name.Matches(name));
            return entry?.InputControl as T;
        }
        return null;
    }
}