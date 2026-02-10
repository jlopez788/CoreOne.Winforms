using CoreOne.Reflection;
using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;
using CoreOne.Winforms.Services.ControlFactories;

namespace Tests.Services.ControlFactories;

public class DropdownControlFactoryTests
{
    private class TestModel
    {
        [DropdownSource(typeof(TestProvider))]
        public string Category { get; set; } = "";

        public string Name { get; set; } = "";
    }

    private class TestProvider
    {
    }

    private DropdownControlFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new DropdownControlFactory();
    }

    [Test]
    public void Priority_Returns100()
    {
        var priority = _factory.Priority;

        Assert.That(priority, Is.EqualTo(100));
    }

    [Test]
    public void CanHandle_PropertyWithDropdownSourceAttribute_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Category));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_PropertyWithoutDropdownSourceAttribute_ReturnsFalse()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.False);
    }

    [Test]
    public void CreateControl_ReturnsComboBox()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Category));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });

        Assert.That(context, Is.Not.Null);
        Assert.That(context!.Control, Is.InstanceOf<ComboBox>());
    }

    [Test]
    public void CreateControl_ComboBoxHasDropDownListStyle()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Category));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var comboBox = (ComboBox)context!.Control;

        Assert.That(comboBox.DropDownStyle, Is.EqualTo(ComboBoxStyle.DropDownList));
    }

    [Test]
    public void CreateControl_SelectedIndexChanged_InvokesCallbackWithDropdownValue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Category));
        var model = new TestModel();
        object? capturedValue = null;

        var context = _factory.CreateControl(property, model, v => capturedValue = v);
        var comboBox = (ComboBox)context!.Control;

        var item1 = new DropdownItem("Display 1", "Value1");
        var item2 = new DropdownItem("Display 2", "Value2");
        comboBox.Items.Add(item1);
        comboBox.Items.Add(item2);

        context.BindEvent();
        comboBox.SelectedItem = item2;

        Assert.That(capturedValue, Is.EqualTo("Value2"));
    }

    [Test]
    public void CreateControl_UpdateControlValue_SelectsMatchingDropdownItem()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Category));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var comboBox = (ComboBox)context!.Control;

        var item1 = new DropdownItem("Display 1", "Value1");
        var item2 = new DropdownItem("Display 2", "Value2");
        var item3 = new DropdownItem("Display 3", "Value3");
        comboBox.Items.Add(item1);
        comboBox.Items.Add(item2);
        comboBox.Items.Add(item3);

        context.UpdateValue("Value2");

        Assert.That(comboBox.SelectedItem, Is.EqualTo(item2));
    }

    [Test]
    public void CreateControl_UpdateControlValueWithNonExistentValue_NoSelectionMade()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Category));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var comboBox = (ComboBox)context!.Control;

        var item1 = new DropdownItem("Display 1", "Value1");
        comboBox.Items.Add(item1);

        context.UpdateValue("NonExistentValue");

        Assert.That(comboBox.SelectedItem, Is.Null);
    }

    [Test]
    public void CreateControl_UpdateControlValueWithNull_NoSelectionMade()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Category));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var comboBox = (ComboBox)context!.Control;

        var item1 = new DropdownItem("Display 1", "Value1");
        comboBox.Items.Add(item1);
        comboBox.SelectedItem = item1;

        context.UpdateValue(null);

        Assert.That(comboBox.SelectedItem, Is.Null);
    }

    [Test]
    public void CreateControl_UpdateControlValue_SelectsFirstMatchingItem()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Category));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var comboBox = (ComboBox)context!.Control;

        var item1 = new DropdownItem("Display 1", "Value1");
        var item2 = new DropdownItem("Display 2", "Value1"); // Same value as item1
        comboBox.Items.Add(item1);
        comboBox.Items.Add(item2);

        context.UpdateValue("Value1");

        Assert.That(comboBox.SelectedItem, Is.EqualTo(item1));
    }

    [Test]
    public void CreateControl_EmptyComboBox_UpdateValueDoesNotThrow()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Category));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });

        Assert.DoesNotThrow(() => context!.UpdateValue("SomeValue"));
    }

    [Test]
    public void CreateControl_SelectedIndexChangedWithoutDropdownItem_DoesNotInvokeCallback()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Category));
        var model = new TestModel();
        var callbackInvoked = false;

        var context = _factory.CreateControl(property, model, _ => callbackInvoked = true);
        var comboBox = (ComboBox)context!.Control;

        comboBox.Items.Add("Plain string item");
        context.BindEvent();
        comboBox.SelectedIndex = 0;

        Assert.That(callbackInvoked, Is.False);
    }

    [Test]
    public void CreateControl_BindAndUnbindEvent_WorksCorrectly()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Category));
        var model = new TestModel();
        var callbackCount = 0;

        var context = _factory.CreateControl(property, model, _ => callbackCount++);
        var comboBox = (ComboBox)context!.Control;

        var item = new DropdownItem("Display", "Value");
        comboBox.Items.Add(item);

        context.BindEvent();
        comboBox.SelectedItem = item;
        Assert.That(callbackCount, Is.EqualTo(1));

        context.UnbindEvent();
        comboBox.SelectedItem = null;
        comboBox.SelectedItem = item;

        Assert.That(callbackCount, Is.EqualTo(1)); // Should not increase after unbind
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);
}
