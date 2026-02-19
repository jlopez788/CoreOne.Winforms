using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;

namespace Tests.Models;

public class ModelContextTests
{
    private class TestModel
    {
        public string Name { get; set; } = "Test";
        public int Age { get; set; } = 25;

        [CoreOne.Winforms.Attributes.Ignore]
        public string IgnoredProperty { get; set; } = "Ignored";
    }

    private class GroupedTestModel
    {
        [Group(1)]
        public string FirstName { get; set; } = "";

        [Group(1)]
        public string LastName { get; set; } = "";

        [Group(2)]
        public string Email { get; set; } = "";

        [Group(2)]
        public string Phone { get; set; } = "";

        public string NoGroup { get; set; } = "";
    }

    private class MultiPriorityGroupModel
    {
        [Group(1)]
        public string HighPriority { get; set; } = "";

        [Group(2)]
        public string MediumPriority { get; set; } = "";

        [Group(3)]
        public string LowPriority { get; set; } = "";
    }

    [Test]
    public void Constructor_WithModel_InitializesProperties()
    {
        var model = new TestModel();

        var context = new ModelContext(model);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(context.Model, Is.SameAs(model));
            Assert.That(context.Properties, Is.Not.Null);
            Assert.That(context.IsModified, Is.False);
        }
    }

    [Test]
    public void Constructor_FiltersIgnoredProperties()
    {
        var model = new TestModel();

        var context = new ModelContext(model);

        var propertyNames = context.Properties.Select(p => p.Name).ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(propertyNames, Does.Contain("Name"));
            Assert.That(propertyNames, Does.Contain("Age"));
            Assert.That(propertyNames, Does.Not.Contain("IgnoredProperty"));
        }
    }

    [Test]
    public void Create_WithNullModel_CreatesNewInstance()
    {
        var context = ModelContext.Create<TestModel>(null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(context, Is.Not.Null);
            Assert.That(context.Model, Is.Not.Null);
            Assert.That(context.Model, Is.InstanceOf<TestModel>());
        }
    }

    [Test]
    public void Create_WithProvidedModel_UsesProvidedInstance()
    {
        var model = new TestModel { Name = "Custom" };

        var context = ModelContext.Create(model);

        Assert.That(context.Model, Is.SameAs(model));
        Assert.That(((TestModel)context.Model).Name, Is.EqualTo("Custom"));
    }

    [Test]
    public void AddGroup_AddsGroupToContext()
    {
        var model = new TestModel();
        var context = new ModelContext(model);
        var group = new GroupDetail(1, "Test Group", 10);

        var result = context.AddGroup(group);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.SameAs(context));
            Assert.That(context.GetGroup(1), Is.SameAs(group));
        }
    }

    [Test]
    public void AddGroup_MultipleGroups_AllAccessible()
    {
        var model = new TestModel();
        var context = new ModelContext(model);
        var group1 = new GroupDetail(1, "Group 1", 10);
        var group2 = new GroupDetail(2, "Group 2", 20);

        context.AddGroup(group1).AddGroup(group2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(context.GetGroup(1), Is.SameAs(group1));
            Assert.That(context.GetGroup(2), Is.SameAs(group2));
        }
    }

    [Test]
    public void AddGroup_DuplicateGroupId_ReplacesExisting()
    {
        var model = new TestModel();
        var context = new ModelContext(model);
        var group1 = new GroupDetail(1, "First", 10);
        var group2 = new GroupDetail(1, "Second", 20);

        context.AddGroup(group1);
        context.AddGroup(group2);

        var retrieved = context.GetGroup(1);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrieved, Is.SameAs(group2));
            Assert.That(retrieved!.Title, Is.EqualTo("Second"));
        }
    }

    [Test]
    public void AddGroups_WithMultipleGroups_AddsAll()
    {
        var model = new TestModel();
        var context = new ModelContext(model);
        var groups = new[]
        {
            new GroupDetail(1, "Group 1"),
            new GroupDetail(2, "Group 2"),
            new GroupDetail(3, "Group 3")
        };

        var result = context.AddGroups(groups);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.SameAs(context));
            Assert.That(context.GetGroup(1), Is.Not.Null);
            Assert.That(context.GetGroup(2), Is.Not.Null);
            Assert.That(context.GetGroup(3), Is.Not.Null);
        }
    }

    [Test]
    public void AddGroups_WithEmptyCollection_DoesNotThrow()
    {
        var model = new TestModel();
        var context = new ModelContext(model);

        Assert.DoesNotThrow(() => context.AddGroups(Array.Empty<GroupDetail>()));
    }

    [Test]
    public void GetGroup_NonExistentGroup_ReturnsDefaultGroup()
    {
        var model = new TestModel();
        var context = new ModelContext(model);

        var result = context.GetGroup(999);

        // Data class has DefaultKey=0, so returns default group for non-existent keys
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.SameAs(GroupDetail.Default));
        }
    }

    [Test]
    public void GetGroup_DefaultGroup_ReturnsDefault()
    {
        var model = new TestModel();
        var context = new ModelContext(model);

        var defaultGroup = context.GetGroup(GroupDetail.GROUP_ID);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(defaultGroup, Is.Not.Null);
            Assert.That(defaultGroup, Is.SameAs(GroupDetail.Default));
        }
    }

    [Test]
    public void GetGroupDetails_OrdersByPriorityDescending()
    {
        var model = new TestModel();
        var context = new ModelContext(model);
        var group1 = new GroupDetail(1, "Low", 5);
        var group2 = new GroupDetail(2, "High", 100);
        var group3 = new GroupDetail(3, "Medium", 50);

        context.AddGroups(new[] { group1, group2, group3 });

        var orderedGroups = context.GetGroupDetails().ToList();

        using (Assert.EnterMultipleScope())
        {
            // Should be ordered: Default (int.MaxValue), High (100), Medium (50), Low (5)
            Assert.That(orderedGroups, Has.Count.EqualTo(4)); // Includes default group
            Assert.That(orderedGroups[0].Priority, Is.EqualTo(int.MaxValue)); // Default
            Assert.That(orderedGroups[1].Priority, Is.EqualTo(100)); // High
            Assert.That(orderedGroups[2].Priority, Is.EqualTo(50)); // Medium
            Assert.That(orderedGroups[3].Priority, Is.EqualTo(5)); // Low
        }
    }

    [Test]
    public void GetGroupDetails_IncludesDefaultGroup()
    {
        var model = new TestModel();
        var context = new ModelContext(model);

        var groups = context.GetGroupDetails().ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(groups, Has.Count.EqualTo(1));
            Assert.That(groups[0], Is.SameAs(GroupDetail.Default));
        }
    }

    [Test]
    public void GetGridColumnSpan_ExistingGroup_ReturnsColumnSpan()
    {
        var model = new TestModel();
        var context = new ModelContext(model);
        var group = new GroupDetail(1, "Test", 0, GridColumnSpan.Half);

        context.AddGroup(group);

        var columnSpan = context.GetGridColumnSpan(1);

        Assert.That(columnSpan, Is.EqualTo(GridColumnSpan.Half));
    }

    [Test]
    public void GetGridColumnSpan_NonExistentGroup_ReturnsDefault()
    {
        var model = new TestModel();
        var context = new ModelContext(model);

        var columnSpan = context.GetGridColumnSpan(999);

        Assert.That(columnSpan, Is.EqualTo(GridColumnSpan.Default));
    }

    [Test]
    public void GetGridColumnSpan_DefaultGroup_ReturnsFullSpan()
    {
        var model = new TestModel();
        var context = new ModelContext(model);

        var columnSpan = context.GetGridColumnSpan(GroupDetail.GROUP_ID);

        Assert.That(columnSpan, Is.EqualTo(GridColumnSpan.Full));
    }

    [Test]
    public void GetGroupEntries_GroupsPropertiesByGroupAttribute()
    {
        var model = new GroupedTestModel();
        var context = new ModelContext(model);

        var entries = context.GetGroupEntries().ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entries, Has.Count.EqualTo(3)); // Group 1, Group 2, Default (NoGroup)

            // Find each group
            var group1 = entries.FirstOrDefault(e => e.GroupId == 1);
            var group2 = entries.FirstOrDefault(e => e.GroupId == 2);
            var defaultGroup = entries.FirstOrDefault(e => e.GroupId == 0);

            Assert.That(group1, Is.Not.Null);
            Assert.That(group2, Is.Not.Null);
            Assert.That(defaultGroup, Is.Not.Null);

            // Verify property counts
            Assert.That(group1!.Properties, Has.Count.EqualTo(2)); // FirstName, LastName
            Assert.That(group2!.Properties, Has.Count.EqualTo(2)); // Email, Phone
            Assert.That(defaultGroup!.Properties, Has.Count.EqualTo(1)); // NoGroup
        }
    }

    [Test]
    public void GetGroupEntries_IncludesPropertyNames()
    {
        var model = new GroupedTestModel();
        var context = new ModelContext(model);

        var entries = context.GetGroupEntries().ToList();
        var group1 = entries.FirstOrDefault(e => e.GroupId == 1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(group1, Is.Not.Null);
            var propertyNames = group1!.Properties.Select(p => p.Name).ToList();
            Assert.That(propertyNames, Does.Contain("FirstName"));
            Assert.That(propertyNames, Does.Contain("LastName"));
        }
    }

    [Test]
    public void GetGroupEntries_OrdersByPriorityDescending()
    {
        var model = new MultiPriorityGroupModel();
        var context = new ModelContext(model);
        
        // Add groups with different priorities
        context.AddGroups(new[]
        {
            new GroupDetail(1, "High", 100),
            new GroupDetail(2, "Medium", 50),
            new GroupDetail(3, "Low", 10)
        });

        var entries = context.GetGroupEntries().ToList();

        using (Assert.EnterMultipleScope())
        {
            // Should be ordered by priority descending: High, Medium, Low
            // No default group because all properties have explicit groups
            Assert.That(entries, Has.Count.EqualTo(3));
            Assert.That(entries[0].Priority, Is.EqualTo(100)); // High
            Assert.That(entries[1].Priority, Is.EqualTo(50)); // Medium
            Assert.That(entries[2].Priority, Is.EqualTo(10)); // Low
        }
    }

    [Test]
    public void GetGroupEntries_WithNoGroupAttribute_UsesDefaultGroup()
    {
        var model = new TestModel();
        var context = new ModelContext(model);

        var entries = context.GetGroupEntries().ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entries, Has.Count.EqualTo(1));
            Assert.That(entries[0].GroupId, Is.EqualTo(0)); // Default group
            Assert.That(entries[0].Properties.Count, Is.GreaterThan(0));
        }
    }

    [Test]
    public void IsModified_InitiallyFalse()
    {
        var model = new TestModel();
        var context = new ModelContext(model);

        Assert.That(context.IsModified, Is.False);
    }

    [Test]
    public void Commit_ResetsIsModified()
    {
        var model = new TestModel();
        var context = new ModelContext(model);
        
        // Simulate modification by notifying property changed
        context.NotifyPropertyChanged(model, "Name", "NewValue");
        Assert.That(context.IsModified, Is.True);

        context.Commit();

        Assert.That(context.IsModified, Is.False);
    }

    [Test]
    public void Rollback_ResetsIsModified()
    {
        var model = new TestModel();
        var context = new ModelContext(model);
        
        // Simulate modification
        context.NotifyPropertyChanged(model, "Name", "NewValue");
        Assert.That(context.IsModified, Is.True);

        context.Rollback();

        Assert.That(context.IsModified, Is.False);
    }

    [Test]
    public void Clear_RemovesRegistrations()
    {
        var model = new TestModel();
        var context = new ModelContext(model);

        context.Clear();

        // Should not throw
        Assert.DoesNotThrow(() => context.NotifyPropertyChanged(model, "Name", "Value"));
    }

    [Test]
    public void GetHashCode_ReturnsModelHashCode()
    {
        var model = new TestModel();
        var context = new ModelContext(model);

        var contextHash = context.GetHashCode();
        var modelHash = model.GetHashCode();

        Assert.That(contextHash, Is.EqualTo(modelHash));
    }

    [Test]
    public void PropertyChanged_IsInitialized()
    {
        var model = new TestModel();
        var context = new ModelContext(model);

        Assert.That(context.PropertyChanged, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithReadOnlyProperties_FiltersCorrectly()
    {
        var model = new ModelWithReadOnlyProperty();
        var context = new ModelContext(model);

        var propertyNames = context.Properties.Select(p => p.Name).ToList();
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(propertyNames, Does.Contain("Name"));
            Assert.That(propertyNames, Does.Not.Contain("ReadOnlyProperty"));
        }
    }

    [Test]
    public void GetGroupEntries_GroupEntryRecord_HasCorrectStructure()
    {
        var model = new GroupedTestModel();
        var context = new ModelContext(model);

        var entry = context.GetGroupEntries().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entry.Properties, Is.Not.Null);
            Assert.That(entry.GroupId, Is.GreaterThanOrEqualTo(0));
        }
    }

    // Helper test models
    private class ModelWithReadOnlyProperty
    {
        public string Name { get; set; } = "";
        public string ReadOnlyProperty { get; } = "ReadOnly";
    }
}
