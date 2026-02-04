using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Tests.Attributes;

[TestFixture]
public class RatingAttributeTests
{
    [Test]
    public void Constructor_WithMaxRating_SetsProperty()
    {
        var attribute = new RatingAttribute(5);
        
        Assert.That(attribute.MaxRating, Is.EqualTo(5));
    }

    [Test]
    public void RatingAttribute_CanBeAppliedToProperty()
    {
        var property = typeof(TestModel).GetProperty(nameof(TestModel.Rating));
        var attribute = property?.GetCustomAttributes(typeof(RatingAttribute), false).FirstOrDefault() as RatingAttribute;
        
        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.MaxRating, Is.EqualTo(5));
    }

    private class TestModel
    {
        [Rating(5)]
        public int Rating { get; set; }
    }
}
