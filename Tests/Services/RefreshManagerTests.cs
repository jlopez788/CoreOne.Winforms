using CoreOne.Winforms.Services;
using CoreOne.Reflection;

namespace Tests.Services;

public class RefreshManagerTests
{
    private RefreshManager _refreshManager = null!;

    [SetUp]
    public void Setup()
    {
        _refreshManager = new RefreshManager();
    }

    [Test]
    public void RegisterContext_StoresContext()
    {
        var model = new TestModel { Name = "Test" };
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var handler = new TestWatchHandler(property, new HashSet<string> { nameof(TestModel.Name) });

        _refreshManager.RegisterContext(handler, model);

        Assert.That(handler.RefreshCalled, Is.True);
    }

    [Test]
    public void NotifyPropertyChanged_RefreshesDependentContexts()
    {
        var model = new TestModel { Name = "Test", DisplayName = "" };
        var nameProperty = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var displayProperty = CreateMetadata(typeof(TestModel), nameof(TestModel.DisplayName));
        
        var handler = new TestWatchHandler(displayProperty, new HashSet<string> { nameof(TestModel.Name) });
        _refreshManager.RegisterContext(handler, model);
        
        handler.RefreshCalled = false;
        _refreshManager.NotifyPropertyChanged(model, nameof(TestModel.Name), "NewName");

        Assert.That(handler.RefreshCalled, Is.True);
    }

    [Test]
    public void NotifyPropertyChanged_WithNoDependencies_DoesNotRefresh()
    {
        var model = new TestModel { Name = "Test" };
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var handler = new TestWatchHandler(property, new HashSet<string>());
        
        _refreshManager.RegisterContext(handler, model);
        handler.RefreshCalled = false;
        
        _refreshManager.NotifyPropertyChanged(model, "OtherProperty", "Value");

        Assert.That(handler.RefreshCalled, Is.False);
    }

    [Test]
    public void Clear_RemovesAllRegistrations()
    {
        var model = new TestModel { Name = "Test" };
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var handler = new TestWatchHandler(property, new HashSet<string> { nameof(TestModel.Name) });
        
        _refreshManager.RegisterContext(handler, model);
        _refreshManager.Clear();
        
        handler.RefreshCalled = false;
        _refreshManager.NotifyPropertyChanged(model, nameof(TestModel.Name), "NewName");

        Assert.That(handler.RefreshCalled, Is.False);
    }

    [Test]
    public void NotifyPropertyChanged_CaseInsensitive_RefreshesContext()
    {
        var model = new TestModel { Name = "Test" };
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.Name));
        var handler = new TestWatchHandler(property, new HashSet<string> { "name" });
        
        _refreshManager.RegisterContext(handler, model);
        handler.RefreshCalled = false;
        
        _refreshManager.NotifyPropertyChanged(model, "NAME", "NewName");

        Assert.That(handler.RefreshCalled, Is.True);
    }

    [Test]
    public void RegisterContext_MultipleDependencies_TracksAll()
    {
        var model = new TestModel { Name = "Test", DisplayName = "Display" };
        var property = CreateMetadata(typeof(TestModel), nameof(TestModel.DisplayName));
        var handler = new TestWatchHandler(property, new HashSet<string> { "Name", "Value" });
        
        _refreshManager.RegisterContext(handler, model);
        handler.RefreshCalled = false;
        
        _refreshManager.NotifyPropertyChanged(model, "Value", 42);

        Assert.That(handler.RefreshCalled, Is.True);
    }

    private static Metadata CreateMetadata(Type type, string propertyName) => MetaType.GetMetadata(type, propertyName);

    private class TestModel
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int Value { get; set; }
    }

    private class TestWatchHandler : CoreOne.Winforms.IWatchHandler
    {
        public Metadata Property { get; }
        public HashSet<string> Dependencies { get; }
        public bool RefreshCalled { get; set; }

        public TestWatchHandler(Metadata property, HashSet<string> dependencies)
        {
            Property = property;
            Dependencies = dependencies;
        }

        public void Refresh(object model)
        {
            RefreshCalled = true;
        }
    }
}
