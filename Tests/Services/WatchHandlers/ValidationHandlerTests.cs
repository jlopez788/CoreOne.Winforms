using CoreOne.Reflection;
using CoreOne.Winforms.Models;
using CoreOne.Winforms.Services.WatchHandlers;
using System.ComponentModel.DataAnnotations;

namespace Tests.Services.WatchHandlers;

public class ValidationHandlerTests
{
    [Test]
    public void CreateInstance_WithValidationAttributes_CreatesHandler()
    {
        var model = new TestModel { Name = "Test" };
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Name), new TextBox());
        var handler = new ValidationHandler();

        var instance = handler.CreateInstance(gridItem);

        Assert.That(instance, Is.Not.Null);
    }

    [Test]
    public void CreateInstance_WithoutValidationAttributes_ReturnsNull()
    {
        var model = new TestModel();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.NoValidation), new TextBox());
        var handler = new ValidationHandler();

        var instance = handler.CreateInstance(gridItem);

        Assert.That(instance, Is.Null);
    }

    [Test]
    public async Task OnRefresh_WithValidValue_ClearsError()
    {
        var model = new TestModel { Name = "Valid Name" };
        var textBox = new TextBox();
        var errorProvider = new ErrorProvider();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Name), textBox, errorProvider);
        var handler = new ValidationHandler();
        var instance = handler.CreateInstance(gridItem);

        instance!.Refresh(model);

        // Wait for debounce
        Thread.Sleep(400);

        var error = errorProvider.GetError(textBox);
        Assert.That(error, Is.Empty);
    }

    [Test]
    public async Task OnRefresh_WithInvalidValue_SetsError()
    {
        var model = new TestModel { Name = "" };
        var textBox = new TextBox();
        var errorProvider = new ErrorProvider();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Name), textBox, errorProvider);
        var handler = new ValidationHandler();
        var instance = handler.CreateInstance(gridItem);

        instance!.Refresh(model);

        // Wait for debounce
        Thread.Sleep(400);

        var error = errorProvider.GetError(textBox);
        Assert.That(error, Is.Not.Empty);
    }

    [Test]
    public async Task OnRefresh_WithMultipleValidationErrors_CombinesErrorMessages()
    {
        var model = new TestModel { Email = "invalid" };
        var textBox = new TextBox();
        var errorProvider = new ErrorProvider();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Email), textBox, errorProvider);
        var handler = new ValidationHandler();
        var instance = handler.CreateInstance(gridItem);

        instance!.Refresh(model);

        // Wait for debounce
        Thread.Sleep(400);

        var error = errorProvider.GetError(textBox);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(error, Is.Not.Empty);
            Assert.That(error, Does.Contain("Invalid email format")); // Email format error
        }
    }

    [Test]
    public void OnRefresh_TransitionFromInvalidToValid_ClearsError()
    {
        var model = new TestModel { Name = "" };
        var textBox = new TextBox();
        var errorProvider = new ErrorProvider();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Name), textBox, errorProvider);
        var handler = new ValidationHandler();
        var instance = handler.CreateInstance(gridItem);

        instance!.Refresh(model);
        Thread.Sleep(400);

        var errorBefore = errorProvider.GetError(textBox);
        Assert.That(errorBefore, Is.Not.Empty);

        // Fix the model
        model.Name = "Valid Name";
        gridItem.ControlContext.UpdateValue("Valid Name");
        instance.Refresh(model);
        Thread.Sleep(500);

        var errorAfter = errorProvider.GetError(textBox);
        Assert.That(errorAfter, Is.Empty);
    }

    [Test]
    public void OnInitialize_WithRequiredAttribute_AddsAsteriskToLabel()
    {
        var model = new TestModel { Name = "Test" };
        var label = new Label { Text = "Name" };
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Name), textBox);
        gridItem.Label = label;
        var handler = new ValidationHandler();
        var instance = handler.CreateInstance(gridItem);

        instance!.Refresh(model);

        Assert.That(label.Text, Does.EndWith(" *"));
    }

    [Test]
    public void OnInitialize_WithoutRequiredAttribute_DoesNotAddAsterisk()
    {
        var model = new TestModel();
        var label = new Label { Text = "No Validation" };
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.NoValidation), textBox);
        gridItem.Label = label;
        var handler = new ValidationHandler();
        var instance = handler.CreateInstance(gridItem);

        // Should return null since no validation attributes
        Assert.That(instance, Is.Null);
    }

    [Test]
    public async Task OnRefresh_WithoutErrorProvider_DoesNotThrow()
    {
        var model = new TestModel { Name = "" };
        var textBox = new TextBox();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Name), textBox, null);
        var handler = new ValidationHandler();
        var instance = handler.CreateInstance(gridItem);

        Assert.DoesNotThrow(() => instance!.Refresh(model));
        Thread.Sleep(400);
    }

    [Test]
    public async Task OnRefresh_WithRangeValidation_ValidatesCorrectly()
    {
        var model = new TestModel { Age = 150 };
        var textBox = new TextBox();
        var errorProvider = new ErrorProvider();
        var gridItem = CreatePropertyGridItem(typeof(TestModel), nameof(TestModel.Age), textBox, errorProvider);
        var handler = new ValidationHandler();
        var instance = handler.CreateInstance(gridItem);

        instance!.Refresh(model);
        Thread.Sleep(400);

        var error = errorProvider.GetError(textBox);
        Assert.That(error, Is.Not.Empty);

        // Valid age
        model.Age = 25;
        instance.Refresh(model);
        Thread.Sleep(500);

        error = errorProvider.GetError(textBox);
        Assert.That(error, Is.Empty);
    }

    private static PropertyGridItem CreatePropertyGridItem(Type type, string propertyName, Control control, ErrorProvider? errorProvider = null)
    {
        var property = CreateMetadata(type, propertyName);
        var contronContext = new EventControlContext(control, "", p => { }, () => { });
        return new PropertyGridItem(contronContext, property, _ => { }) {
            Label = new Label { Text = propertyName },
            ErrorProvider = errorProvider
        };
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);

    private class TestModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, ErrorMessage = "Name must be less than 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Range(0, 120, ErrorMessage = "Age must be between 0 and 120")]
        public int Age { get; set; }

        public string NoValidation { get; set; } = string.Empty;
    }
}