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

    [Test]
    public void CreateControl_UpdateControlValueWithNull_UsesCurrentDate()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.ModifiedDate));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var picker = (DateTimePicker)context!.Control;
        var originalValue = picker.Value;

        context.UpdateValue(null);

        // Should still have a valid date (DateTime can't be null in DateTimePicker)
        Assert.That(picker.Value, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void CreateControl_BindAndUnbindEvent_WorksCorrectly()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.CreatedDate));
        var model = new TestModel();
        var callbackCount = 0;

        var context = _factory.CreateControl(property, model, _ => callbackCount++);
        var picker = (DateTimePicker)context!.Control;

        context.BindEvent();
        picker.Value = new DateTime(2025, 1, 15);
        Assert.That(callbackCount, Is.EqualTo(1));

        context.UnbindEvent();
        picker.Value = new DateTime(2025, 1, 16);

        Assert.That(callbackCount, Is.EqualTo(1)); // Should not increase after unbind
    }

    [Test]
    public void CreateControl_WithMinAndMaxValues_HandlesEdgeCases()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.CreatedDate));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var picker = (DateTimePicker)context!.Control;

        // Test updating with very old date
        var oldDate = new DateTime(1900, 1, 1);
        context.UpdateValue(oldDate);
        Assert.That(picker.Value, Is.EqualTo(oldDate));

        // Test updating with future date
        var futureDate = new DateTime(2099, 12, 31);
        context.UpdateValue(futureDate);
        Assert.That(picker.Value, Is.EqualTo(futureDate));
    }

    [Test]
    public void CreateControl_Dispose_DisposesControl()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.CreatedDate));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });

        Assert.DoesNotThrow(() => context!.Dispose());
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);

    private class TestModel
    {
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Name { get; set; } = "";
    }
}