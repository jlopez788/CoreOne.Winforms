using CoreOne.Reflection;
using CoreOne.Winforms;
using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;
using CoreOne.Winforms.Services;
using Moq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tests.Services;

public class PropertyGridItemFactoryTests
{
    private Mock<IControlFactory> _mockControlFactory = null!;
    private PropertyGridItemFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _mockControlFactory = new Mock<IControlFactory>();
        _factory = new PropertyGridItemFactory();
    }

    [Test]
    public void CreatePropertyGridItem_WithHandledProperty_CreatesItem()
    {
        var model = new TestModel { Name = "Test" };
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var textBox = new TextBox();

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem, Is.Not.Null);
        Assert.That(gridItem!.InputControl, Is.SameAs(textBox));
        Assert.That(gridItem.Label, Is.Not.Null);
        Assert.That(gridItem.Container, Is.Not.Null);
    }

    [Test]
    public void CreatePropertyGridItem_WithUnhandledProperty_ReturnsNull()
    {
        var model = new TestModel();
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(false);

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem, Is.Null);
    }

    [Test]
    public void CreatePropertyGridItem_WithNullModel_ThrowsArgumentNullException()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        Assert.Throws<ArgumentNullException>(() =>
            _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, null!, _ => { }));
    }

    [Test]
    public void CreatePropertyGridItem_WithNullOnValueChanged_ThrowsArgumentNullException()
    {
        var model = new TestModel();
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        Assert.Throws<ArgumentNullException>(() =>
            _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, null!));
    }

    [Test]
    public void CreatePropertyGridItem_UsesDisplayAttribute()
    {
        var model = new TestModelWithAttributes();
        var property = CreateMetadata(typeof(TestModelWithAttributes), nameof(TestModelWithAttributes.FirstName));
        var textBox = new TextBox();

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem!.Label.Text, Is.EqualTo("First Name"));
    }

    [Test]
    public void CreatePropertyGridItem_UsesDisplayNameAttribute()
    {
        var model = new TestModelWithAttributes();
        var property = CreateMetadata(typeof(TestModelWithAttributes), nameof(TestModelWithAttributes.LastName));
        var textBox = new TextBox();

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem!.Label.Text, Is.EqualTo("Last Name"));
    }

    [Test]
    public void CreatePropertyGridItem_UsesDescriptionAttribute()
    {
        var model = new TestModelWithAttributes();
        var property = CreateMetadata(typeof(TestModelWithAttributes), nameof(TestModelWithAttributes.EmailAddress));
        var textBox = new TextBox();

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem!.Label.Text, Is.EqualTo("Email"));
    }

    [Test]
    public void CreatePropertyGridItem_FormatsPropertyNameWithSpaces()
    {
        var model = new TestModelWithAttributes();
        var property = CreateMetadata(typeof(TestModelWithAttributes), nameof(TestModelWithAttributes.PhoneNumber));
        var textBox = new TextBox();

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem!.Label.Text, Is.EqualTo("Phone Number"));
    }

    [Test]
    public void CreatePropertyGridItem_WithGridColumnAttribute_SetsColumnSpan()
    {
        var model = new TestModelWithAttributes();
        var property = CreateMetadata(typeof(TestModelWithAttributes), nameof(TestModelWithAttributes.FullWidthField));
        var textBox = new TextBox();

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem!.ColumnSpan, Is.EqualTo(GridColumnSpan.Full));
    }

    [Test]
    public void CreatePropertyGridItem_WithVisibleAttributeFalse_SetsNoneColumnSpan()
    {
        var model = new TestModelWithAttributes();
        var property = CreateMetadata(typeof(TestModelWithAttributes), nameof(TestModelWithAttributes.HiddenField));
        var textBox = new TextBox();

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem!.ColumnSpan, Is.EqualTo(GridColumnSpan.None));
    }

    [Test]
    public void CreatePropertyGridItem_SetsInitialValue()
    {
        var model = new TestModel { Name = "InitialValue" };
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var textBox = new TextBox();
        var setValueCalled = false;
        object? capturedValue = null;

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", value => {
                setValueCalled = true;
                capturedValue = value;
            }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(setValueCalled, Is.True);
        Assert.That(capturedValue, Is.EqualTo("InitialValue"));
    }

    [Test]
    public void CreatePropertyGridItem_ConfiguresControlDocking()
    {
        var model = new TestModel();
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var textBox = new TextBox();

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem!.InputControl.Dock, Is.EqualTo(DockStyle.Top));
        Assert.That(gridItem.InputControl.Height, Is.EqualTo(23));
    }

    [Test]
    public void CreatePropertyGridItem_ConfiguresLabelProperties()
    {
        var model = new TestModel { Name = "Test" };
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var textBox = new TextBox();

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem!.Label.AutoSize, Is.True);
        Assert.That(gridItem.Label.Dock, Is.EqualTo(DockStyle.Top));
        Assert.That(gridItem.Label.Height, Is.EqualTo(21));
    }

    [Test]
    public void CreatePropertyGridItem_ConfiguresContainerPanel()
    {
        var model = new TestModel();
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var textBox = new TextBox();

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem!.Container.Dock, Is.EqualTo(DockStyle.Fill));
        Assert.That(gridItem.Container.AutoSize, Is.True);
        Assert.That(gridItem.Container.MinimumSize.Height, Is.EqualTo(45));
    }

    [Test]
    public void CreatePropertyGridItem_AddsControlsToContainer()
    {
        var model = new TestModel();
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var textBox = new TextBox();

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem!.Container.Controls.Count, Is.EqualTo(2));
        Assert.That(gridItem.Container.Controls.Contains(gridItem.InputControl), Is.True);
        Assert.That(gridItem.Container.Controls.Contains(gridItem.Label), Is.True);
    }

    [Test]
    public void CreatePropertyGridItem_WhenFactoryReturnsNull_ReturnsNull()
    {
        var model = new TestModel();
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Returns((EventControlContext?)null);

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(gridItem, Is.Null);
    }

    [Test]
    public void CreatePropertyGridItem_PassesOnValueChangedToFactory()
    {
        var model = new TestModel();
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var textBox = new TextBox();
        Action<object?>? capturedCallback = null;

        _mockControlFactory.Setup(f => f.CanHandle(property)).Returns(true);
        _mockControlFactory.Setup(f => f.CreateControl(property, model, It.IsAny<Action<object?>>()))
            .Callback<Metadata, object, Action<object?>>((p, m, cb) => capturedCallback = cb)
            .Returns(new EventControlContext(textBox, "TextChanged", _ => { }, () => { }));

        var gridItem = _factory.CreatePropertyGridItem(_mockControlFactory.Object, property, model, _ => { });

        Assert.That(capturedCallback, Is.Not.Null);
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);

    private class TestModel
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private class TestModelWithAttributes
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [DisplayName("Last Name")]
        public string LastName { get; set; } = "";

        [System.ComponentModel.Description("Email")]
        public string EmailAddress { get; set; } = "";

        public string PhoneNumber { get; set; } = "";

        [GridColumn(GridColumnSpan.Full)]
        public string FullWidthField { get; set; } = "";

        [Visible(false)]
        public string HiddenField { get; set; } = "";
    }
}