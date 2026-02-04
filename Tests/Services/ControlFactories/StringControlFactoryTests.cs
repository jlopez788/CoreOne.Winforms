using CoreOne.Winforms.Services.ControlFactories;
using CoreOne.Reflection;
using System.Windows.Forms;

namespace Tests.Services.ControlFactories;

public class StringControlFactoryTests
{
    private StringControlFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new StringControlFactory();
    }

    [Test]
    public void CanHandle_StringProperty_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_NonStringProperty_ReturnsFalse()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Age));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.False);
    }

    [Test]
    public void CreateControl_ReturnsTextBox()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var model = new TestModel();
        var valueChanged = false;
        object? capturedValue = null;

        var context = _factory.CreateControl(property, model, v => { valueChanged = true; capturedValue = v; });

        Assert.That(context, Is.Not.Null);
        Assert.That(context!.Control, Is.InstanceOf<TextBox>());
    }

    [Test]
    public void CreateControl_TextChanged_InvokesCallback()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var model = new TestModel();
        object? capturedValue = null;

        var context = _factory.CreateControl(property, model, v => capturedValue = v);
        var textBox = (TextBox)context!.Control;

        context.BindEvent();
        textBox.Text = "NewValue";

        Assert.That(capturedValue, Is.EqualTo("NewValue"));
    }

    [Test]
    public void CreateControl_UpdateControlValue_SetsText()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var textBox = (TextBox)context!.Control;

        context.UpdateValue("TestValue");

        Assert.That(textBox.Text, Is.EqualTo("TestValue"));
    }

    [Test]
    public void CreateControl_UpdateControlValueWithNull_SetsEmptyString()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var textBox = (TextBox)context!.Control;

        context.UpdateValue(null);

        Assert.That(textBox.Text, Is.EqualTo(string.Empty));
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);

    private class TestModel
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }
}
