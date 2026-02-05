using CoreOne.Attributes;
using CoreOne.Reflection;
using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;
using CoreOne.Winforms.Services.WatchHandlers;
using System.Reflection;
using System.Windows.Forms;

namespace Tests.Services.WatchHandlers;

public class EnabledWhenHandlerTests
{
    private class TestModel
    {
        public int Age { get; set; }
        [EnabledWhen(nameof(Age), 18, ComparisonType.LessThan)]
        public string AgeDependent { get; set; } = "";
        [EnabledWhen(nameof(IsActive), true)]
        public string DependentField { get; set; } = "";
        public bool IsActive { get; set; }
        public string Name { get; set; } = "";
        [EnabledWhen(nameof(Age), 65, ComparisonType.GreaterThan)]
        public string SeniorField { get; set; } = "";
    }

    private EnabledWhenHandler _factory = null!;

    [Test]
    public void CreateInstance_WithEnabledWhenAttribute_ReturnsHandler()
    {
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.DependentField), new TextBox());

        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void CreateInstance_WithoutEnabledWhenAttribute_ReturnsNull()
    {
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Name), new TextBox());

        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Null);
    }

    [SetUp]
    public void Setup()
    {
        _factory = new EnabledWhenHandler();
    }

    [Test]
    public void WatchHandler_DependsOnCorrectProperty()
    {
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.DependentField), new TextBox());

        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler!.Dependencies, Does.Contain(nameof(TestModel.IsActive)));
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);

    private static PropertyGridItem CreatePropertyGridItem(Type type, string propertyName, Control control)
    {
        var metadata = CreateMetadata(type, propertyName);
        var controlContext = new ControlContext(control, "", p => { }, () => { });
        return new PropertyGridItem(controlContext, metadata, _ => { });
    }
}