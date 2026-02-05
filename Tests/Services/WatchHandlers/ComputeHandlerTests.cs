using CoreOne.Reflection;
using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;
using CoreOne.Winforms.Services.WatchHandlers;

namespace Tests.Services.WatchHandlers;

public class ComputeHandlerTests
{
    private class TestModel
    {
        public string FirstName { get; set; } = "";
        [Compute(nameof(GetFullName))]
        [WatchProperties(nameof(FirstName), nameof(LastName))]
        public string FullName { get; set; } = "";
        public string LastName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        [Compute(nameof(GetTimestamp))]
        public string Timestamp { get; set; } = "";

        [Compute(nameof(CalculateTotal))]
        [WatchProperties(nameof(Quantity), nameof(Price))]
        public decimal Total { get; set; }

        public decimal CalculateTotal(decimal quantity, decimal price) => quantity * price;

        public string GetFullName() => $"{FirstName} {LastName}";

        public string GetTimestamp() => DateTime.Now.ToString("yyyy-MM-dd");
    }

    private ComputeHandler _factory = null!;

    [Test]
    public void CreateInstance_UsingMethodParameters()
    {
        var model = new TestModel {
            FirstName = "Unit",
            LastName = "Test",
            Quantity = 2,
            Price = 2.5m
        };
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Total), new TextBox(), model);

        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Not.Null);

        handler.Refresh(model);
        Assert.That(model.Total, Is.EqualTo(5m));
    }

    [Test]
    public void CreateInstance_WithComputeAttribute_ReturnsHandler()
    {
        var model = new TestModel {
            FirstName = "Unit",
            LastName = "Test"
        };
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.FullName), new TextBox(), model);

        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Not.Null);

        handler.Refresh(model);
        Assert.That(model.FullName, Is.EqualTo("Unit Test"));
    }

    [Test]
    public void CreateInstance_WithoutComputeAttribute_ReturnsNull()
    {
        var model = new TestModel();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.FirstName), new TextBox(), model);

        var handler = _factory.CreateInstance(gridItem);

        Assert.That(handler, Is.Null);
    }

    [SetUp]
    public void Setup()
    {
        _factory = new ComputeHandler();
    }

    [Test]
    public void WatchHandler_DependsOnCorrectProperties()
    {
        var model = new TestModel();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.FullName), new TextBox(), model);

        var handler = _factory.CreateInstance(gridItem);

        using (Assert.EnterMultipleScope())
        {
            Assert.Multiple(() => {
                Assert.That(handler!.Dependencies, Does.Contain(nameof(TestModel.FirstName)));
                Assert.That(handler.Dependencies, Does.Contain(nameof(TestModel.LastName)));
            });
        }
    }

    private static PropertyGridItem CreatePropertyGridItem(Type type, string propertyName, TextBox control, object model)
    {
        var metadata = MetaType.GetMetadata(type, propertyName);
        var controlContext = new ControlContext(control, "", p => metadata.SetValue(model, p), () => { });
        return new PropertyGridItem(controlContext, metadata, value => {
            control.Text = value?.ToString() ?? "";
            metadata.SetValue(model, value);
        });
    }
}