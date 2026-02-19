using CoreOne.Winforms.Models;

namespace Tests.Models;

public class GroupDetailTests
{
    [Test]
    public void Constructor_WithAllParameters_SetsProperties()
    {
        var groupDetail = new GroupDetail(1, "Test Group", 10, GridColumnSpan.Half);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(groupDetail.GroupId, Is.EqualTo(1));
            Assert.That(groupDetail.Title, Is.EqualTo("Test Group"));
            Assert.That(groupDetail.Priority, Is.EqualTo(10));
            Assert.That(groupDetail.ColumnSpan, Is.EqualTo(GridColumnSpan.Half));
        }
    }

    [Test]
    public void Constructor_WithDefaultParameters_UsesDefaults()
    {
        var groupDetail = new GroupDetail(2, "Simple Group");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(groupDetail.GroupId, Is.EqualTo(2));
            Assert.That(groupDetail.Title, Is.EqualTo("Simple Group"));
            Assert.That(groupDetail.Priority, Is.EqualTo(0));
            Assert.That(groupDetail.ColumnSpan, Is.EqualTo(GridColumnSpan.Full));
        }
    }

    [Test]
    public void Constructor_WithPriorityOnly_UsesDefaultColumnSpan()
    {
        var groupDetail = new GroupDetail(3, "Priority Group", 5);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(groupDetail.GroupId, Is.EqualTo(3));
            Assert.That(groupDetail.Title, Is.EqualTo("Priority Group"));
            Assert.That(groupDetail.Priority, Is.EqualTo(5));
            Assert.That(groupDetail.ColumnSpan, Is.EqualTo(GridColumnSpan.Full));
        }
    }

    [Test]
    public void GroupId_IsReadOnly()
    {
        var property = typeof(GroupDetail).GetProperty(nameof(GroupDetail.GroupId));

        Assert.That(property, Is.Not.Null);
        Assert.That(property!.CanWrite, Is.False);
    }

    [Test]
    public void Title_IsReadOnly()
    {
        var property = typeof(GroupDetail).GetProperty(nameof(GroupDetail.Title));

        Assert.That(property, Is.Not.Null);
        Assert.That(property!.CanWrite, Is.False);
    }

    [Test]
    public void Priority_IsReadOnly()
    {
        var property = typeof(GroupDetail).GetProperty(nameof(GroupDetail.Priority));

        Assert.That(property, Is.Not.Null);
        Assert.That(property!.CanWrite, Is.False);
    }

    [Test]
    public void ColumnSpan_IsReadOnly()
    {
        var property = typeof(GroupDetail).GetProperty(nameof(GroupDetail.ColumnSpan));

        Assert.That(property, Is.Not.Null);
        Assert.That(property!.CanWrite, Is.False);
    }

    [Test]
    public void Default_HasCorrectProperties()
    {
        var defaultGroup = GroupDetail.Default;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(defaultGroup.GroupId, Is.EqualTo(GroupDetail.GROUP_ID));
            Assert.That(defaultGroup.Title, Is.EqualTo("Default"));
            Assert.That(defaultGroup.Priority, Is.EqualTo(int.MaxValue));
            Assert.That(defaultGroup.ColumnSpan, Is.EqualTo(GridColumnSpan.Full));
        }
    }

    [Test]
    public void Default_IsSingleton()
    {
        var default1 = GroupDetail.Default;
        var default2 = GroupDetail.Default;

        Assert.That(default1, Is.SameAs(default2));
    }

    [Test]
    public void GROUP_ID_IsZero()
    {
        Assert.That(GroupDetail.GROUP_ID, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithNegativeGroupId_AcceptsValue()
    {
        var groupDetail = new GroupDetail(-1, "Negative Group");

        Assert.That(groupDetail.GroupId, Is.EqualTo(-1));
    }

    [Test]
    public void Constructor_WithNegativePriority_AcceptsValue()
    {
        var groupDetail = new GroupDetail(1, "Group", -10);

        Assert.That(groupDetail.Priority, Is.EqualTo(-10));
    }

    [Test]
    public void Constructor_WithEmptyTitle_AcceptsValue()
    {
        var groupDetail = new GroupDetail(1, "");

        Assert.That(groupDetail.Title, Is.Empty);
    }

    [Test]
    public void Constructor_WithDifferentColumnSpans_SetsCorrectly()
    {
        var testCases = new[]
        {
            GridColumnSpan.None,
            GridColumnSpan.One,
            GridColumnSpan.Two,
            GridColumnSpan.Half,
            GridColumnSpan.Four,
            GridColumnSpan.Five,
            GridColumnSpan.Full
        };

        foreach (var columnSpan in testCases)
        {
            var groupDetail = new GroupDetail(1, "Test", 0, columnSpan);
            Assert.That(groupDetail.ColumnSpan, Is.EqualTo(columnSpan));
        }
    }

    [Test]
    public void TwoInstances_WithSameValues_AreNotSame()
    {
        var group1 = new GroupDetail(1, "Test", 5, GridColumnSpan.Half);
        var group2 = new GroupDetail(1, "Test", 5, GridColumnSpan.Half);

        Assert.That(group1, Is.Not.SameAs(group2));
    }

    [Test]
    public void Constructor_WithMaxPriority_SetsCorrectly()
    {
        var groupDetail = new GroupDetail(1, "Max Priority", int.MaxValue);

        Assert.That(groupDetail.Priority, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void Constructor_WithMinPriority_SetsCorrectly()
    {
        var groupDetail = new GroupDetail(1, "Min Priority", int.MinValue);

        Assert.That(groupDetail.Priority, Is.EqualTo(int.MinValue));
    }
}
