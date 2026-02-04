using CoreOne.Winforms.Services.ControlFactories;
using CoreOne.Reflection;
using System.Windows.Forms;

namespace Tests.Services.ControlFactories;

public class DateTimeControlFactoryTests
{
    private DateTimeControlFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new DateTimeControlFactory();
    }

    [Test]
    public void CanHandle_DateTimeProperty_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.CreatedDate));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_NullableDateTimeProperty_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.ModifiedDate));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_NonDateTimeProperty_ReturnsFalse()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.False);
    }

    [Test]
    public void CreateControl_ReturnsDateTimePicker()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.CreatedDate));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });

        Assert.That(context, Is.Not.Null);
        Assert.That(context!.Control, Is.InstanceOf<DateTimePicker>());
    }

    [Test]
    public void CreateControl_ValueChanged_InvokesCallback()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.CreatedDate));
        var model = new TestModel();
        object? capturedValue = null;

        var context = _factory.CreateControl(property, model, v => capturedValue = v);
        context?.BindEvent();
        var picker = (DateTimePicker)context!.Control;
        var newDate = new DateTime(2025, 1, 15);

        picker.Value = newDate;

        Assert.That(capturedValue, Is.EqualTo(newDate));
    }

    [Test]
    public void CreateControl_UpdateControlValue_SetsValue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.CreatedDate));
        var model = new TestModel();
        var testDate = new DateTime(2025, 1, 15);

        var context = _factory.CreateControl(property, model, _ => { });
        var picker = (DateTimePicker)context!.Control;

        context.UpdateValue(testDate);

        Assert.That(picker.Value, Is.EqualTo(testDate));
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);

    private class TestModel
    {
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Name { get; set; } = "";
    }
}
