using CoreOne.Winforms.Attributes;

namespace Tests.Attributes;

public class GroupAttributeTests
{
    [Test]
    public void Constructor_WithTitle_SetsTitle()
    {
        var attribute = new GroupAttribute(1);

        Assert.That(attribute.GroupId, Is.EqualTo(1));
    }

    [Test]
    public void Title_IsReadOnly()
    {
        var attribute = new GroupAttribute(1);
        var property = typeof(GroupAttribute).GetProperty(nameof(GroupAttribute.GroupId));

        Assert.That(property, Is.Not.Null);
        Assert.That(property!.CanWrite, Is.False);
    }

    [Test]
    public void AttributeUsage_AllowsSingleInstance()
    {
        var attributeUsage = typeof(GroupAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        Assert.That(attributeUsage, Is.Not.Null);
        Assert.That(attributeUsage!.AllowMultiple, Is.False);
    }

    [Test]
    public void AttributeUsage_TargetsProperties()
    {
        var attributeUsage = typeof(GroupAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        Assert.That(attributeUsage, Is.Not.Null);
        Assert.That(attributeUsage!.ValidOn, Is.EqualTo(AttributeTargets.Property));
    }

    [Test]
    public void ApplyToProperty_CanBeRetrieved()
    {
        var property = typeof(TestModel).GetProperty(nameof(TestModel.FirstName));
        var attribute = property?.GetCustomAttributes(typeof(GroupAttribute), false)
            .Cast<GroupAttribute>()
            .FirstOrDefault();

        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.GroupId, Is.EqualTo(1));
    }

    [Test]
    public void MultipleProperties_CanHaveSameGroup()
    {
        var firstNameProp = typeof(TestModel).GetProperty(nameof(TestModel.FirstName));
        var lastNameProp = typeof(TestModel).GetProperty(nameof(TestModel.LastName));

        var attr1 = firstNameProp?.GetCustomAttributes(typeof(GroupAttribute), false)
            .Cast<GroupAttribute>()
            .FirstOrDefault();
        var attr2 = lastNameProp?.GetCustomAttributes(typeof(GroupAttribute), false)
            .Cast<GroupAttribute>()
            .FirstOrDefault();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attr1, Is.Not.Null);
            Assert.That(attr2, Is.Not.Null);
            Assert.That(attr1!.GroupId, Is.EqualTo(attr2!.GroupId));
        }
    }

    [Test]
    public void PropertyWithoutAttribute_ReturnsNull()
    {
        var property = typeof(TestModel).GetProperty(nameof(TestModel.NoGroup));
        var attribute = property?.GetCustomAttributes(typeof(GroupAttribute), false)
            .Cast<GroupAttribute>()
            .FirstOrDefault();

        Assert.That(attribute, Is.Null);
    }

    [Test]
    public void DifferentGroups_HaveDifferentTitles()
    {
        var personalProp = typeof(TestModel).GetProperty(nameof(TestModel.FirstName));
        var contactProp = typeof(TestModel).GetProperty(nameof(TestModel.Email));

        var personalAttr = personalProp?.GetCustomAttributes(typeof(GroupAttribute), false)
            .Cast<GroupAttribute>()
            .FirstOrDefault();
        var contactAttr = contactProp?.GetCustomAttributes(typeof(GroupAttribute), false)
            .Cast<GroupAttribute>()
            .FirstOrDefault();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(personalAttr, Is.Not.Null);
            Assert.That(contactAttr, Is.Not.Null);
            Assert.That(personalAttr!.GroupId, Is.Not.EqualTo(contactAttr!.GroupId));
        }
    }

    // Test model
    private class TestModel
    {
        [Group(1)]
        public string FirstName { get; set; } = string.Empty;

        [Group(1)]
        public string LastName { get; set; } = string.Empty;

        [Group(2)]
        public string Email { get; set; } = string.Empty;

        public string NoGroup { get; set; } = string.Empty;
    }
}