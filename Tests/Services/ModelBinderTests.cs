using CoreOne;
using CoreOne.Reactive;
using CoreOne.Reflection;
using CoreOne.Winforms;
using CoreOne.Winforms.Events;
using CoreOne.Winforms.Models;
using CoreOne.Winforms.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Tests.Services;

public class ModelBinderTests
{
    private class TestModel
    {
        public int Age { get; set; }
        public string Name { get; set; } = "";
    }

    private class TestModelWithIgnored
    {
        public string Name { get; set; } = "";

        [CoreOne.Winforms.Attributes.Ignore]
        public string Secret { get; set; } = "";
    }

    private class TestModelWithUnsupportedType
    {
        public DateTime UnsupportedProperty { get; set; }
    }

    private class TestNumericControlFactory : IControlFactory
    {
        public int Priority => 0;

        public bool CanHandle(Metadata property) => property.FPType == typeof(int);

        public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
        {
            var numericUpDown = new NumericUpDown();
            numericUpDown.Name = property.Name;
            return new EventControlContext(numericUpDown,
                "ValueChanged",
                value => numericUpDown.Value = value is int i ? i : 0,
                () => onValueChanged((int)numericUpDown.Value));
        }
    }

    // Test implementations
    private class TestStringControlFactory : IControlFactory
    {
        public int Priority => 0;

        public bool CanHandle(Metadata property) => property.FPType == typeof(string);

        public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
        {
            var textBox = new TextBox();
            textBox.Name = property.Name;

            return new EventControlContext(textBox,
                "TextChanged",
                value => textBox.Text = value?.ToString() ?? "",
                () => onValueChanged(textBox.Text));
        }
    }

    private ModelBinder _binder = null!;
    private Mock<IGridLayoutManager> _mockLayoutManager = null!;
    private IServiceProvider _serviceProvider = null!;

    [Test]
    public void BindModel_CallsLayoutManagerCorrectly()
    {
        var model = new TestModel { Name = "Test" };
        var context = new ModelContext(model);

        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 150));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 150));

        _binder.BindModel(context);

        _mockLayoutManager.Verify(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()), Times.AtLeastOnce);
        _mockLayoutManager.Verify(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()), Times.Once);
        _mockLayoutManager.Verify(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()), Times.Once);
    }

    [Test]
    public void BindModel_FiltersIgnoredProperties()
    {
        var model = new TestModelWithIgnored { Name = "Test", Secret = "Hidden" };
        var context = new ModelContext(model);

        var capturedSpans = new List<(Control?, GridColumnSpan)>();
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control?, GridColumnSpan)>>(spans => capturedSpans.AddRange(spans))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(context);

        // Should only bind Name property (Secret is ignored)
        Assert.That(capturedSpans.Count, Is.EqualTo(1));
    }

    [Test]
    public void BindModel_RegistersWatchHandlersWithContext()
    {
        var mockWatchFactory = new Mock<IWatchFactory>();
        var mockWatchHandler = new Mock<IWatchHandler>();

        mockWatchFactory.Setup(f => f.Priority).Returns(100);
        mockWatchFactory.Setup(f => f.CreateInstance(It.IsAny<PropertyGridItem>()))
            .Returns(mockWatchHandler.Object);
        mockWatchHandler.Setup(h => h.Dependencies).Returns([]);

        var services = new ServiceCollection();
        services.AddSingleton<IControlFactory>(new TestStringControlFactory());
        services.AddSingleton<IWatchFactory>(mockWatchFactory.Object);
        services.AddSingleton<IPropertyGridItemFactory, PropertyGridItemFactory>();

        using var serviceProvider = services.BuildServiceProvider();
        using var binder = new ModelBinder(serviceProvider, _mockLayoutManager.Object);

        var model = new TestModel { Name = "Test" };
        var context = new ModelContext(model);

        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        binder.BindModel(context);

        mockWatchHandler.Verify(h => h.Refresh(model), Times.AtLeastOnce);
    }

    [Test]
    public void BindModel_ReturnsPanel()
    {
        var model = new TestModel { Name = "Test" };
        var context = new ModelContext(model);

        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        var panel = _binder.BindModel(context);

        Assert.That(panel, Is.Not.Null);
        Assert.That(panel, Is.InstanceOf<Panel>());
    }

    [Test]
    public void BindModel_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _binder.BindModel(null!));
    }

    [Test]
    public void BindModel_WithPropertyWithoutFactory_SkipsProperty()
    {
        // Model with property type that no factory can handle
        var model = new TestModelWithUnsupportedType { UnsupportedProperty = DateTime.Now };
        var context = new ModelContext(model);

        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        // Should not throw, just skip the unsupported property
        Assert.DoesNotThrow(() => _binder.BindModel(context));
    }

    [Test]
    public void BindModel_WithValidModel_BindsSuccessfully()
    {
        var model = new TestModel { Name = "Test", Age = 25 };
        var context = new ModelContext(model);

        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        var panel = _binder.BindModel(context);

        Assert.That(panel, Is.Not.Null);
        Assert.That(panel, Is.TypeOf<TableLayoutPanel>());
    }

    [Test]
    public void Dispose_DisposesPropertyChangedSubject()
    {
        var binder = new ModelBinder(_serviceProvider, _mockLayoutManager.Object);

        Assert.DoesNotThrow(() => binder.Dispose());
    }

    [Test]
    public void ModelContext_CommitWorks()
    {
        var model = new TestModel { Name = "Test" };
        var context = new ModelContext(model);

        // Modify the model
        model.Name = "Modified";
        context.NotifyPropertyChanged(model, nameof(TestModel.Name), "Modified");

        Assert.That(context.IsModified, Is.True);

        context.Commit();

        Assert.That(context.IsModified, Is.False);
    }

    [Test]
    public void ModelContext_RollbackWorks()
    {
        var model = new TestModel { Name = "Original" };
        var context = new ModelContext(model);

        // Modify the model
        model.Name = "Modified";
        context.NotifyPropertyChanged(model, nameof(TestModel.Name), "Modified");

        Assert.That(context.IsModified, Is.True);

        context.Rollback();

        Assert.That(context.IsModified, Is.False);
        Assert.That(model.Name, Is.EqualTo("Original"));
    }

    [Test]
    public void PropertyChanged_FiresWhenValueChanges()
    {
        ModelPropertyChanged? capturedArgs = null;
        var model = new TestModel { Name = "Initial" };
        var context = new ModelContext(model);
        var binder = new ModelBinder(_serviceProvider, new GridLayoutManager());
        using var token = SToken.Create();
        // Subscribe by wrapping in Observer.Create
        binder.PropertyChanged.Subscribe(args => {
            capturedArgs = args;
        }, token);

        var panel = binder.BindModel(context);

        // Simulate property change through control - need to find the textbox in the rendered panel
        var textBox = FindTextBoxInPanel(panel);
        if (textBox != null)
        {
            textBox.Text = "Updated";
        }

        System.Threading.Thread.Sleep(50); // Allow async operations

        Assert.That(capturedArgs, Is.Not.Null);
        Assert.That(capturedArgs!.NewValue, Is.EqualTo("Updated"));
    }

    #region Grouping Tests

    private class GroupedTestModel
    {
        [CoreOne.Winforms.Attributes.Group(1)]
        public string FirstName { get; set; } = "";

        [CoreOne.Winforms.Attributes.Group(1)]
        public string LastName { get; set; } = "";

        [CoreOne.Winforms.Attributes.Group(2)]
        public string Email { get; set; } = "";

        public string NoGroup { get; set; } = "";
    }

    [Test]
    public void BindModel_WithGroupedProperties_CreatesGroupBoxes()
    {
        var model = new GroupedTestModel();
        var context = new ModelContext(model);
        context.AddGroups(new[]
        {
            new GroupDetail(1, "Personal Info", 10, GridColumnSpan.Half),
            new GroupDetail(2, "Contact Info", 5, GridColumnSpan.Half)
        });

        var capturedGroups = new List<(Control control, GridColumnSpan columnSpan)>();
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control, GridColumnSpan)>>(groups => capturedGroups.AddRange(groups))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(context);

        using (Assert.EnterMultipleScope())
        {
            // Should have 3 groups: Group 1 (GroupBox), Group 2 (GroupBox), Default (Panel)
            Assert.That(capturedGroups, Has.Count.EqualTo(3));

            // Group 1 and 2 should be GroupBox
            var groupBoxes = capturedGroups.Where(g => g.control is GroupBox).ToList();
            Assert.That(groupBoxes, Has.Count.EqualTo(2));

            // Default group should be TableLayoutPanel
            var defaultPanels = capturedGroups.Where(g => g.control is TableLayoutPanel).ToList();
            Assert.That(defaultPanels, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void BindModel_WithGroupedProperties_SetsGroupBoxTitle()
    {
        var model = new GroupedTestModel();
        var context = new ModelContext(model);
        context.AddGroup(new GroupDetail(1, "Personal Information", 10));

        var capturedGroups = new List<(Control control, GridColumnSpan columnSpan)>();
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control, GridColumnSpan)>>(groups => capturedGroups.AddRange(groups))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(context);

        var groupBox = capturedGroups
            .Select(g => g.control)
            .OfType<GroupBox>()
            .FirstOrDefault(gb => gb.Text == "Personal Information");

        Assert.That(groupBox, Is.Not.Null);
    }

    [Test]
    public void BindModel_WithDefaultGroup_DoesNotCreateGroupBox()
    {
        var model = new TestModel { Name = "Test" };
        var context = new ModelContext(model);

        var capturedGroups = new List<(Control control, GridColumnSpan columnSpan)>();
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control, GridColumnSpan)>>(groups => capturedGroups.AddRange(groups))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(context);

        // Default group should not create GroupBox, only TableLayoutPanel
        using (Assert.EnterMultipleScope())
        {
            Assert.That(capturedGroups, Has.Count.EqualTo(1));
            Assert.That(capturedGroups[0].control, Is.TypeOf<TableLayoutPanel>());
        }
    }

    [Test]
    public void BindModel_WithGroupedProperties_RespectsColumnSpan()
    {
        var model = new GroupedTestModel();
        var context = new ModelContext(model);
        context.AddGroups(new[]
        {
            new GroupDetail(1, "Group 1", 10, GridColumnSpan.Half),
            new GroupDetail(2, "Group 2", 5, GridColumnSpan.Full)
        });

        var capturedGroups = new List<(Control control, GridColumnSpan columnSpan)>();
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control, GridColumnSpan)>>(groups => capturedGroups.AddRange(groups))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(context);

        using (Assert.EnterMultipleScope())
        {
            // Find the group boxes and check their column spans
            var group1Span = capturedGroups.FirstOrDefault(g => g.control is GroupBox gb && gb.Text == "Group 1").columnSpan;
            var group2Span = capturedGroups.FirstOrDefault(g => g.control is GroupBox gb && gb.Text == "Group 2").columnSpan;

            Assert.That(group1Span, Is.EqualTo(GridColumnSpan.Half));
            Assert.That(group2Span, Is.EqualTo(GridColumnSpan.Full));
        }
    }

    [Test]
    public void BindModel_WithEmptyGroup_DoesNotRenderGroup()
    {
        var model = new TestModel { Name = "Test" };
        var context = new ModelContext(model);
        // Add a group but no properties have this group ID
        context.AddGroup(new GroupDetail(99, "Empty Group", 10));

        var capturedGroups = new List<(Control control, GridColumnSpan columnSpan)>();
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control, GridColumnSpan)>>(groups => capturedGroups.AddRange(groups))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(context);

        // Should only have the default group
        Assert.That(capturedGroups, Has.Count.EqualTo(1));
    }

    [Test]
    public void BindModel_WithGroupButNoGroupDetail_UsesDefaultGroupTitle()
    {
        var model = new GroupedTestModel();
        var context = new ModelContext(model);
        // Don't add GroupDetail for group 1

        var capturedGroups = new List<(Control control, GridColumnSpan columnSpan)>();
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control, GridColumnSpan)>>(groups => capturedGroups.AddRange(groups))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(context);

        var groupBox = capturedGroups
            .Select(g => g.control)
            .OfType<GroupBox>()
            .FirstOrDefault();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(groupBox, Is.Not.Null);
            // Since Data has DefaultKey, non-existent groups return Default group with title "Default"
            Assert.That(groupBox!.Text, Is.EqualTo("Default"));
        }
    }

    [Test]
    public void BindModel_GroupBoxCreatedForNonDefaultGroup()
    {
        var model = new GroupedTestModel();
        var context = new ModelContext(model);
        context.AddGroup(new GroupDetail(1, "Test Group", 10));
        context.AddGroup(new GroupDetail(2, "Contact Info", 5));

        var capturedGroups = new List<(Control control, GridColumnSpan columnSpan)>();
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control, GridColumnSpan)>>(groups => capturedGroups.AddRange(groups))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(context);

        var groupBoxes = capturedGroups
            .Select(g => g.control)
            .OfType<GroupBox>()
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(groupBoxes, Has.Count.GreaterThanOrEqualTo(1));
            var testGroupBox = groupBoxes.FirstOrDefault(gb => gb.Text == "Test Group");
            Assert.That(testGroupBox, Is.Not.Null, "Expected to find a GroupBox with title 'Test Group'");
        }
    }

    [Test]
    public void BindModel_GroupBoxContainsTableLayoutPanel()
    {
        var model = new GroupedTestModel();
        var context = new ModelContext(model);
        context.AddGroup(new GroupDetail(1, "Personal Info", 10));
        context.AddGroup(new GroupDetail(2, "Contact Info", 5));

        var capturedGroups = new List<(Control control, GridColumnSpan columnSpan)>();
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control, GridColumnSpan)>>(groups => capturedGroups.AddRange(groups))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(context);

        var groupBox = capturedGroups
            .Select(g => g.control)
            .OfType<GroupBox>()
            .FirstOrDefault(gb => gb.Text == "Personal Info");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(groupBox, Is.Not.Null);
            // GroupBox should have correct properties
            Assert.That(groupBox!.Text, Is.EqualTo("Personal Info"));
            Assert.That(groupBox.Size.Width, Is.GreaterThan(0));
            Assert.That(groupBox.Size.Height, Is.GreaterThan(0));
        }
    }

    [Test]
    public void BindModel_GroupsOrderedByPriority()
    {
        var model = new MultiPriorityGroupModel();
        var context = new ModelContext(model);
        context.AddGroups(new[]
        {
            new GroupDetail(1, "High Priority", 100),
            new GroupDetail(2, "Medium Priority", 50),
            new GroupDetail(3, "Low Priority", 10)
        });

        var capturedGroups = new List<(Control control, GridColumnSpan columnSpan)>();
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 100));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control, GridColumnSpan)>>(groups => capturedGroups.AddRange(groups))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(context);

        var groupBoxes = capturedGroups
            .Select(g => g.control)
            .OfType<GroupBox>()
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(groupBoxes, Has.Count.EqualTo(3));
            // Groups should be ordered by priority (descending)
            Assert.That(groupBoxes[0].Text, Is.EqualTo("High Priority"));
            Assert.That(groupBoxes[1].Text, Is.EqualTo("Medium Priority"));
            Assert.That(groupBoxes[2].Text, Is.EqualTo("Low Priority"));
        }
    }

    [Test]
    public void BindModel_GroupWithZeroHeight_NotAddedToLayout()
    {
        var model = new GroupedTestModel();
        var context = new ModelContext(model);
        context.AddGroup(new GroupDetail(1, "Group 1", 10));

        var capturedGroups = new List<(Control control, GridColumnSpan columnSpan)>();
        // Return height=0 for groups (simulating empty group)
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<PropertyGridItem>>()))
            .Returns((new TableLayoutPanel(), 0));
        _mockLayoutManager.Setup(m => m.CalculateLayout(It.IsAny<IEnumerable<(Control, GridColumnSpan)>>()))
            .Callback<IEnumerable<(Control, GridColumnSpan)>>(groups => capturedGroups.AddRange(groups))
            .Returns(new List<GridCell>());
        _mockLayoutManager.Setup(m => m.RenderLayout(It.IsAny<IEnumerable<GridCell>>()))
            .Returns((new TableLayoutPanel(), 100));

        _binder.BindModel(context);

        // No groups should be captured since all had height=0
        Assert.That(capturedGroups, Is.Empty);
    }

    [Test]
    public void BindModel_WithRealLayoutManager_CreatesGroupBoxCorrectly()
    {
        var model = new GroupedTestModel();
        var context = new ModelContext(model);
        context.AddGroup(new GroupDetail(1, "Personal Info", 10));
        context.AddGroup(new GroupDetail(2, "Contact Info", 5));

        var realLayoutManager = new GridLayoutManager();
        var binder = new ModelBinder(_serviceProvider, realLayoutManager);

        var result = binder.BindModel(context);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<TableLayoutPanel>());
            
            // The outer TableLayoutPanel should contain GroupBoxes and/or panels
            var outerPanel = (TableLayoutPanel)result;
            Assert.That(outerPanel.Controls.Count, Is.GreaterThan(0));
            
            // Should have at least one GroupBox for non-default groups
            var hasGroupBox = outerPanel.Controls.Cast<Control>().Any(c => c is GroupBox);
            Assert.That(hasGroupBox, Is.True, "Expected to find at least one GroupBox in the layout");
        }

        binder.Dispose();
    }

    private class MultiPriorityGroupModel
    {
        [CoreOne.Winforms.Attributes.Group(1)]
        public string HighPriority { get; set; } = "";

        [CoreOne.Winforms.Attributes.Group(2)]
        public string MediumPriority { get; set; } = "";

        [CoreOne.Winforms.Attributes.Group(3)]
        public string LowPriority { get; set; } = "";
    }

    #endregion

    [SetUp]
    public void Setup()
    {
        _mockLayoutManager = new Mock<IGridLayoutManager>();

        var services = new ServiceCollection();

        // Register control factories
        services.AddSingleton<IControlFactory>(new TestStringControlFactory());
        services.AddSingleton<IControlFactory>(new TestNumericControlFactory());
        services.AddSingleton<IEnumerable<IWatchFactory>>(new List<IWatchFactory>());
        services.AddSingleton<IPropertyGridItemFactory>(new PropertyGridItemFactory());
        _serviceProvider = services.BuildServiceProvider();

        _binder = new ModelBinder(_serviceProvider, _mockLayoutManager.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _binder?.Dispose();
        (_serviceProvider as IDisposable)?.Dispose();
    }

    private static TextBox? FindTextBoxInPanel(Panel panel)
    {
        // Recursively search for TextBox in the panel hierarchy
        foreach (Control control in panel.Controls)
        {
            if (control is TextBox textBox)
                return textBox;

            if (control is Panel childPanel)
            {
                var found = FindTextBoxInPanel(childPanel);
                if (found != null)
                    return found;
            }

            if (control is TableLayoutPanel tablePanel)
            {
                foreach (Control child in tablePanel.Controls)
                {
                    if (child is TextBox tb)
                        return tb;
                    if (child is Panel cp)
                    {
                        var found = FindTextBoxInPanel(cp);
                        if (found != null)
                            return found;
                    }
                }
            }
        }
        return null;
    }
}