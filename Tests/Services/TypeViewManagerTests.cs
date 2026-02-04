using CoreOne.Winforms.Services;
using CoreOne.Winforms.Controls;
using System.Reflection;

namespace Tests.Services;

public class TypeViewManagerTests
{
    [Test]
    public void Constructor_WithoutServiceProvider_CreatesInstance()
    {
        var manager = new TypeViewManager();
        
        Assert.That(manager, Is.Not.Null);
    }

    [Test]
    public void Set_AndResolve_ReturnsView()
    {
        var manager = new TypeViewManager(type => new TestView());
        manager.Set("TestView", typeof(TestView));

        var view = manager.Resolve("TestView");

        Assert.That(view, Is.Not.Null);
        Assert.That(view, Is.InstanceOf<CoreOne.Winforms.IView>());
    }

    [Test]
    public void Resolve_WithViewSuffix_RemovesSuffix()
    {
        var manager = new TypeViewManager(type => new TestView());
        manager.Set("Test", typeof(TestView));

        var view = manager.Resolve("TestView");

        Assert.That(view, Is.Not.Null);
    }

    [Test]
    public void Resolve_WithoutViewSuffix_FindsMatchWhenRegistered()
    {
        var manager = new TypeViewManager(type => new TestView());
        manager.Set("Test", typeof(TestView)); // Register without "View" suffix

        var view = manager.Resolve("Test");

        Assert.That(view, Is.Not.Null);
    }

    [Test]
    public void Resolve_NonExistentView_ReturnsNull()
    {
        var manager = new TypeViewManager(type => new TestView());

        var view = manager.Resolve("NonExistent");

        Assert.That(view, Is.Null);
    }

    [Test]
    public void Resolve_NullName_ReturnsNull()
    {
        var manager = new TypeViewManager(type => new TestView());

        var view = manager.Resolve(null);

        Assert.That(view, Is.Null);
    }

    [Test]
    public void RegisterViews_ScansAssembly()
    {
        var manager = new TypeViewManager(type => new TestView());
        var assembly = Assembly.GetExecutingAssembly();

        manager.RegisterViews(assembly);

        // Verify manager can now resolve TestView
        var view = manager.Resolve("Test");
        Assert.That(view, Is.Not.Null);
    }

    [Test]
    public void TryGetValue_ExistingKey_ReturnsTrue()
    {
        var manager = new TypeViewManager(type => new TestView());
        manager.Set("TestView", typeof(TestView));

        var exists = manager.TryGetValue("TestView", out var type);

        Assert.Multiple(() => {
            Assert.That(exists, Is.True);
            Assert.That(type, Is.EqualTo(typeof(TestView)));
        });
    }

    [Test]
    public void ContainsKey_ExistingKey_ReturnsTrue()
    {
        var manager = new TypeViewManager(type => new TestView());
        manager.Set("TestView", typeof(TestView));

        Assert.That(manager.ContainsKey("TestView"), Is.True);
    }

    [Test]
    public void Remove_ExistingKey_RemovesView()
    {
        var manager = new TypeViewManager(type => new TestView());
        manager.Set("TestView", typeof(TestView));

        manager.Remove("TestView");

        Assert.That(manager.ContainsKey("TestView"), Is.False);
    }

    public class TestView : BaseView
    {
        public TestView()
        {
            // Empty constructor for testing
        }
    }
}
