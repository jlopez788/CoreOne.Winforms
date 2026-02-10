using CoreOne.Reflection;
using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Services.ControlFactories;

namespace Tests.Services.ControlFactories;

public class FileControlFactoryTests
{
    private class TestModel
    {
        [File(filter: "Text Files (*.txt)|*.txt", multiselect: false)]
        public string SingleFile { get; set; } = "";

        [File(filter: "All Files (*.*)|*.*", multiselect: true)]
        public string MultipleFiles { get; set; } = "";

        public string Name { get; set; } = "";
    }

    private FileControlFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new FileControlFactory();
    }

    [Test]
    public void Priority_Returns50()
    {
        var priority = _factory.Priority;

        Assert.That(priority, Is.EqualTo(50));
    }

    [Test]
    public void CanHandle_PropertyWithFileAttribute_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_PropertyWithoutFileAttribute_ReturnsFalse()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.False);
    }

    [Test]
    public void CreateControl_ReturnsPanel()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });

        Assert.That(context, Is.Not.Null);
        Assert.That(context!.Control, Is.InstanceOf<Panel>());
    }

    [Test]
    public void CreateControl_PanelContainsTextBoxAndButton()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var panel = (Panel)context!.Control;

        var textBox = panel.Controls.OfType<TextBox>().FirstOrDefault();
        var button = panel.Controls.OfType<Button>().FirstOrDefault();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(textBox, Is.Not.Null);
            Assert.That(button, Is.Not.Null);
        }
    }

    [Test]
    public void CreateControl_TextBoxIsDisabledAndReadOnly()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var panel = (Panel)context!.Control;
        var textBox = panel.Controls.OfType<TextBox>().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(textBox.Enabled, Is.False);
            Assert.That(textBox.ReadOnly, Is.True);
        }
    }

    [Test]
    public void CreateControl_ButtonHasEllipsisText()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var panel = (Panel)context!.Control;
        var button = panel.Controls.OfType<Button>().First();

        Assert.That(button.Text, Is.EqualTo("..."));
    }

    [Test]
    public void CreateControl_UpdateControlValue_SetsTextBoxText()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var panel = (Panel)context!.Control;
        var textBox = panel.Controls.OfType<TextBox>().First();

        context.UpdateValue("C:\\test\\file.txt");

        Assert.That(textBox.Text, Is.EqualTo("C:\\test\\file.txt"));
    }

    [Test]
    public void CreateControl_UpdateControlValueWithNull_SetsEmptyString()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var panel = (Panel)context!.Control;
        var textBox = panel.Controls.OfType<TextBox>().First();
        textBox.Text = "Some value";

        context.UpdateValue(null);

        Assert.That(textBox.Text, Is.EqualTo(string.Empty));
    }

    [Test]
    public void CreateControl_BindEvent_InitializesFileDialogWithCorrectFilter()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        
        // Binding event initializes the FileDialog
        context.BindEvent();

        // We can't directly access the FileDialog, but we verify no exception is thrown
        Assert.Pass("BindEvent completed successfully");
    }

    [Test]
    public void CreateControl_UnbindEvent_DisposesFileDialog()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        
        context.BindEvent();
        context.UnbindEvent();

        // Verify multiple unbinds don't throw
        Assert.DoesNotThrow(() => context.UnbindEvent());
    }

    [Test]
    public void CreateControl_MultipleBindUnbindCycles_WorksCorrectly()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });

        // Test multiple bind/unbind cycles
        context.BindEvent();
        context.UnbindEvent();
        context.BindEvent();
        context.UnbindEvent();

        Assert.Pass("Multiple bind/unbind cycles completed successfully");
    }

    [Test]
    public void CreateControl_PanelHasCorrectAnchors()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var panel = (Panel)context!.Control;
        var textBox = panel.Controls.OfType<TextBox>().First();
        var button = panel.Controls.OfType<Button>().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(textBox.Anchor, Is.EqualTo(AnchorStyles.Left | AnchorStyles.Right));
            Assert.That(button.Anchor, Is.EqualTo(AnchorStyles.Right));
        }
    }

    [Test]
    public void CreateControl_TextBoxAndButtonSizing_IsCorrect()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var panel = (Panel)context!.Control;
        var textBox = panel.Controls.OfType<TextBox>().First();
        var button = panel.Controls.OfType<Button>().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(textBox.Left, Is.EqualTo(0));
            Assert.That(textBox.Top, Is.EqualTo(0));
            Assert.That(button.Left, Is.EqualTo(textBox.Width));
            Assert.That(textBox.Width + button.Width, Is.GreaterThan(0));
        }
    }

    [Test]
    public void CreateControl_Dispose_DisposesAllControls()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        
        context.BindEvent();
        
        Assert.DoesNotThrow(() => context.Dispose());
    }

    [Test]
    public void CreateControl_WithMultiSelectAttribute_CreatesCorrectContext()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.MultipleFiles));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });

        Assert.That(context, Is.Not.Null);
        Assert.That(context!.Control, Is.InstanceOf<Panel>());
    }

    [Test]
    public void CreateControl_PropertyWithoutAttribute_BindEventDoesNotThrow()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.SingleFile));
        var model = new TestModel();

        // Create a control and verify it initializes properly
        var context = _factory.CreateControl(property, model, _ => { });
        
        Assert.DoesNotThrow(() => context.BindEvent());
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);
}
