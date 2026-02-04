using CoreOne.Winforms.Services.ControlFactories;
using CoreOne.Winforms.Attributes;
using CoreOne.Reflection;
using System.Windows.Forms;

namespace Tests.Services.ControlFactories;

public class RatingControlFactoryTests
{
    private RatingControlFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new RatingControlFactory();
    }

    [Test]
    public void CanHandle_IntPropertyWithRatingAttribute_ReturnsTrue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Rating));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_IntPropertyWithoutRatingAttribute_ReturnsFalse()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Age));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.False);
    }

    [Test]
    public void CanHandle_NonIntProperty_ReturnsFalse()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));

        var canHandle = _factory.CanHandle(property);

        Assert.That(canHandle, Is.False);
    }

    [Test]
    public void CreateControl_ReturnsRatingControl()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Rating));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });

        Assert.That(context, Is.Not.Null);
        Assert.That(context!.Control, Is.InstanceOf<CoreOne.Winforms.Controls.RatingControl>());
    }

    [Test]
    public void CreateControl_SetsMaxRatingFromAttribute()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Rating));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var ratingControl = (CoreOne.Winforms.Controls.RatingControl)context!.Control;

        Assert.That(ratingControl.MaxRating, Is.EqualTo(5));
    }

    [Test]
    public void CreateControl_ValueChanged_InvokesCallback()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Rating));
        var model = new TestModel();
        object? capturedValue = null;

        var context = _factory.CreateControl(property, model, v => capturedValue = v);
        var ratingControl = (CoreOne.Winforms.Controls.RatingControl)context!.Control;

        context.BindEvent();
        ratingControl.Value = 4;

        Assert.That(capturedValue, Is.EqualTo(4));
    }

    [Test]
    public void CreateControl_UpdateControlValue_SetsValue()
    {
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Rating));
        var model = new TestModel();

        var context = _factory.CreateControl(property, model, _ => { });
        var ratingControl = (CoreOne.Winforms.Controls.RatingControl)context!.Control;

        context.UpdateValue(3);

        Assert.That(ratingControl.Value, Is.EqualTo(3));
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);

    private class TestModel
    {
        [Rating(5)]
        public int Rating { get; set; }
        
        public int Age { get; set; }
        public string Name { get; set; } = "";
    }
}
