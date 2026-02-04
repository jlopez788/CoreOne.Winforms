using CoreOne.Winforms.Services.ControlFactories;
using CoreOne.Reflection;
using System.Windows.Forms;
using RangeAttribute = System.ComponentModel.DataAnnotations.RangeAttribute;

namespace Tests.Services.ControlFactories;

public class NumericControlFactoryTests
{
    private NumericControlFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new NumericControlFactory();
    }

    [Test]
    public void CanHandle_IntProperty_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Age));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_DecimalProperty_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Price));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_ByteProperty_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.ByteValue));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_StringProperty_ReturnsFalse()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.False);
    }

    [Test]
    public void CreateControl_ReturnsNumericUpDown()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Age));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });

        Assert.That(context, Is.Not.Null);
        Assert.That(context!.Control, Is.InstanceOf<NumericUpDown>());
    }

    [Test]
    public void CreateControl_IntProperty_SetsIntegerRange()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Age));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var numericUpDown = (NumericUpDown)context!.Control;

        Assert.Multiple(() => {
            Assert.That(numericUpDown.Minimum, Is.EqualTo(int.MinValue));
            Assert.That(numericUpDown.Maximum, Is.EqualTo(int.MaxValue));
        });
    }

    [Test]
    public void CreateControl_ByteProperty_SetsByteRange()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.ByteValue));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var numericUpDown = (NumericUpDown)context!.Control;

        Assert.Multiple(() => {
            Assert.That(numericUpDown.Minimum, Is.EqualTo(0));
            Assert.That(numericUpDown.Maximum, Is.EqualTo(255));
        });
    }

    [Test]
    public void CreateControl_DecimalProperty_SetsDecimalPlaces()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Price));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var numericUpDown = (NumericUpDown)context!.Control;

        Assert.That(numericUpDown.DecimalPlaces, Is.EqualTo(2));
    }

    [Test]
    public void CreateControl_WithRangeAttribute_UsesRangeValues()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.RangedValue));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var numericUpDown = (NumericUpDown)context!.Control;

        Assert.Multiple(() => {
            Assert.That(numericUpDown.Minimum, Is.EqualTo(1));
            Assert.That(numericUpDown.Maximum, Is.EqualTo(100));
        });
    }

    [Test]
    public void CreateControl_ValueChanged_InvokesCallback()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Age));
        var model = new TestModel();
        object? capturedValue = null;

        var context = _factory.CreateControl(property, model, v => capturedValue = v);
        var numericUpDown = (NumericUpDown)context!.Control;

        context.BindEvent();
        numericUpDown.Value = 25;

        Assert.That(capturedValue, Is.EqualTo(25));
    }

    [Test]
    public void CreateControl_UpdateControlValue_SetsValue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Age));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var numericUpDown = (NumericUpDown)context!.Control;

        context.UpdateValue(42);

        Assert.That(numericUpDown.Value, Is.EqualTo(42));
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);

    private class TestModel
    {
        public int Age { get; set; }
        public decimal Price { get; set; }
        public byte ByteValue { get; set; }
        public string Name { get; set; } = "";

        [Range(1, 100)]
        public int RangedValue { get; set; }
    }
}