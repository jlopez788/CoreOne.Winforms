using CoreOne.Winforms.Services.ControlFactories;
using CoreOne.Reflection;
using System.Windows.Forms;

namespace Tests.Services.ControlFactories;

public class EnumControlFactoryTests
{
    private EnumControlFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new EnumControlFactory();
    }

    [Test]
    public void CanHandle_EnumProperty_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Status));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_NullableEnumProperty_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.OptionalStatus));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_NonEnumProperty_ReturnsFalse()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.False);
    }

    [Test]
    public void CreateControl_ReturnsComboBox()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Status));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });

        Assert.That(context, Is.Not.Null);
        Assert.That(context!.Control, Is.InstanceOf<ComboBox>());
    }

    [Test]
    public void CreateControl_PopulatesEnumValues()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Status));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var comboBox = (ComboBox)context!.Control;

        Assert.That(comboBox.Items.Count, Is.EqualTo(3));
        Assert.Multiple(() => {
            Assert.That(comboBox.Items.Contains(TestStatus.Active), Is.True);
            Assert.That(comboBox.Items.Contains(TestStatus.Inactive), Is.True);
            Assert.That(comboBox.Items.Contains(TestStatus.Pending), Is.True);
        });
    }

    [Test]
    public void CreateControl_UpdateControlValue_SetsSelectedItem()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Status));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var comboBox = (ComboBox)context!.Control;

        context.UpdateValue(TestStatus.Active);

        Assert.That(comboBox.SelectedItem, Is.EqualTo(TestStatus.Active));
    }

    [Test]
    public void CreateControl_SetsSelectedItem()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Status));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var comboBox = (ComboBox)context!.Control;
        
        context.UpdateValue(TestStatus.Active);

        Assert.That(comboBox.SelectedItem, Is.EqualTo(TestStatus.Active));
    }

    private static Metadata CreateMetadata(Type type, string propertyName)
    {
        var propInfo = type.GetProperty(propertyName)!;
        return new Metadata(propInfo, propInfo.PropertyType, null, null);
    }

    private enum TestStatus
    {
        Active,
        Inactive,
        Pending
    }

    private class TestModel
    {
        public TestStatus Status { get; set; }
        public TestStatus? OptionalStatus { get; set; }
        public string Name { get; set; } = "";
    }
}
