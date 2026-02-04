using CoreOne.Winforms.Services.ControlFactories;
using CoreOne.Reflection;
using System.Windows.Forms;

namespace Tests.Services.ControlFactories;

public class BooleanControlFactoryTests
{
    private BooleanControlFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new BooleanControlFactory();
    }

    [Test]
    public void CanHandle_BooleanProperty_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.IsActive));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_NullableBooleanProperty_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.IsOptional));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_NonBooleanProperty_ReturnsFalse()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.False);
    }

    [Test]
    public void CreateControl_ReturnsCheckBox()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.IsActive));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });

        Assert.That(context, Is.Not.Null);
        Assert.That(context!.Control, Is.InstanceOf<CheckBox>());
    }

    [Test]
    public void CreateControl_CheckedChanged_InvokesCallback()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.IsActive));
        var model = new TestModel();
        object? capturedValue = null;

        var context = _factory.CreateControl(property, model, v => capturedValue = v);
        var checkBox = (CheckBox)context!.Control;

        context.BindEvent();
        checkBox.Checked = true;

        Assert.That(capturedValue, Is.EqualTo(true));
    }

    [Test]
    public void CreateControl_UpdateControlValueWithTrue_ChecksBox()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.IsActive));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var checkBox = (CheckBox)context!.Control;

        context.UpdateValue(true);

        Assert.That(checkBox.Checked, Is.True);
    }

    [Test]
    public void CreateControl_UpdateControlValueWithFalse_UnchecksBox()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.IsActive));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var checkBox = (CheckBox)context!.Control;
        checkBox.Checked = true;

        context.UpdateValue(false);

        Assert.That(checkBox.Checked, Is.False);
    }

    [Test]
    public void CreateControl_UpdateControlValueWithNull_UnchecksBox()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.IsActive));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var checkBox = (CheckBox)context!.Control;
        checkBox.Checked = true;

        context.UpdateValue(null);

        Assert.That(checkBox.Checked, Is.False);
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);

    private class TestModel
    {
        public bool IsActive { get; set; }
        public bool? IsOptional { get; set; }
        public string Name { get; set; } = "";
    }
}