using CoreOne.Attributes;
using CoreOne.Extensions;
using CoreOne.Models;
using CoreOne.Reflection;
using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;
using CoreOne.Winforms.Services.WatchHandlers;

namespace Tests.Services.WatchHandlers;

public class ClearWhenHandlerTests
{
    private class TestModel
    {
        public int Age { get; set; }
        public string Country { get; set; } = "";

        [ClearWhen(nameof(Country), "USA", ComparisonType.NotEqualTo)]
        public string State { get; set; } = "";

        [ClearWhen(nameof(Age), 18, ComparisonType.LessThan)]
        public string AdultField { get; set; } = "";

        public string Name { get; set; } = "";
    }

    private ClearWhenHandler _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new ClearWhenHandler();
    }

    [Test]
    public void CreateInstance_WithClearWhenAttribute_ReturnsHandler()
    {
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.State), textBox, p => textBox.Text = p?.ToString());

        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void CreateInstance_WithoutClearWhenAttribute_ReturnsNull()
    {
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Name), textBox, p => textBox.Text = p?.ToString());

        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Null);
    }

    [Test]
    public void WatchHandler_DependsOnCorrectProperty()
    {
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.State), textBox, p => textBox.Text = p?.ToString());

        var handler = _factory.CreateInstance(gridItem);
        Assert.That(handler, Is.Not.Null);
        
        handler.Refresh(new TestModel { Country = "Test" }); // Trigger dependency tracking

        Assert.That(handler!.Dependencies, Does.Contain(nameof(TestModel.Country)));
    }

    [Test]
    public void Refresh_WhenConditionMet_ClearsValue()
    {
        var model = new TestModel { Country = "Canada", State = "Ontario" };
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.State), textBox, p => textBox.Text = p?.ToString());
        var handler = _factory.CreateInstance(gridItem);

        handler!.Refresh(model);

        Assert.That(model.State, Is.Null);
    }

    [Test]
    public void Refresh_WhenConditionNotMet_KeepsValue()
    {
        var model = new TestModel { Country = "USA", State = "California" };
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.State), textBox, p => textBox.Text = p?.ToString());
        var handler = _factory.CreateInstance(gridItem);

        handler!.Refresh(model);

        Assert.That(model.State, Is.EqualTo("California"));
    }

    [Test]
    public void Refresh_WithLessThanComparison_ClearsWhenBelow()
    {
        var model = new TestModel { Age = 15, AdultField = "Some Value" };
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.AdultField), textBox, p => textBox.Text = p?.ToString());
        var handler = _factory.CreateInstance(gridItem);

        handler!.Refresh(model);

        Assert.That(model.AdultField, Is.Null);
    }

    [Test]
    public void Refresh_WithLessThanComparison_KeepsWhenAbove()
    {
        var model = new TestModel { Age = 21, AdultField = "Adult Value" };
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.AdultField), textBox, p => textBox.Text = p?.ToString());
        var handler = _factory.CreateInstance(gridItem);

        handler!.Refresh(model);

        Assert.That(model.AdultField, Is.EqualTo("Adult Value"));
    }

    [Test]
    public void Refresh_ClearsValueAndUpdatesControl()
    {
        var model = new TestModel { Country = "Canada", State = "Ontario" };
        var textBox = new TextBox { Text = "Ontario" };
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.State), textBox, p => textBox.Text = p?.ToString());
        var handler = _factory.CreateInstance(gridItem);

        handler!.Refresh(model);
        Thread.Sleep(50); // Allow cross-thread operation to complete

        using (Assert.EnterMultipleScope())
        {
            Assert.That(model.State, Is.Null);
            Assert.That(textBox.Text, Is.EqualTo(string.Empty));
        }
    }

    [Test]
    public void Refresh_FirstRefresh_InitializesCorrectly()
    {
        var model = new TestModel { Country = "Mexico", State = "Test" };
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.State), textBox, p => textBox.Text = p?.ToString());
        var handler = _factory.CreateInstance(gridItem);

        // First refresh should initialize and process
        handler!.Refresh(model);

        Assert.That(model.State, Is.Null);
    }

    [Test]
    public void WatchHandler_MultipleDependenciesTracked()
    {
        var model = new TestModel { Age = 15 };
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.AdultField), textBox, p => textBox.Text = p?.ToString());
        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Not.Null);
        handler.Refresh(model);

        Assert.That(handler!.Dependencies, Does.Contain(nameof(TestModel.Age)));
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);

    private static PropertyGridItem CreatePropertyGridItem(Type type, string propertyName, Control control, Action<object?>? setValue)
    {
        var metadata = CreateMetadata(type, propertyName);
        setValue ??= _ => { };
        var controlContext = new EventControlContext(control, "", setValue, () => { });
        return new PropertyGridItem(controlContext, metadata, setValue);
    }
}