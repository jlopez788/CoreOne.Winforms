## Project Overview

**CoreOne.Winforms** is a modern WinForms library providing dynamic model-based form generation with automatic two-way data binding, extensible handler pipeline, themed controls, and smooth animations. Built for .NET 9.0.

### Key Features
- Dynamic form generation from model properties via attributes
- 6-column Bootstrap-style grid layout system
- Priority-based factory pattern for control creation
- Reactive property watching and cascading updates
- Attribute-driven UI behaviors (EnabledWhen, Compute, Validation, etc.)
- Dependency injection throughout
- Built on SOLID & DRY principles

## Architecture & Design Principles

### SOLID Principles (Enforced)

#### Single Responsibility Principle
- Each class has one clear purpose (e.g., `StringControlFactory` only handles string controls)
- Factories handle only their specific type/attribute
- Services manage only their designated concern

#### Open/Closed Principle
- Extensible via interfaces (`IControlFactory`, `IWatchFactory`, `IDropdownSourceProvider`)
- New functionality added by implementing interfaces, not modifying existing code
- Priority-based chain of responsibility allows behavior customization

#### Liskov Substitution Principle
- All implementations properly substitute their interfaces
- Base classes (`WatchHandler`, `ControlContext`, `Animation`) provide proper abstractions
- No surprising behavior when using derived types

#### Interface Segregation Principle
- Small, focused interfaces (e.g., `IControlFactory`, `IRefreshManager`, `IView`)
- Clients depend only on interfaces they use
- Separate sync/async providers: `IDropdownSourceProviderSync` vs `IDropdownSourceProviderAsync`

#### Dependency Inversion Principle
- High-level modules depend on abstractions, not concretions
- Constructor injection used throughout (e.g., `ModelBinder(IServiceProvider, IRefreshManager, IGridLayoutManager)`)
- Service provider manages dependencies

### DRY Principle (Enforced)

#### Base Classes for Shared Behavior
```csharp
// ✅ Template method pattern - lifecycle hooks
public abstract class WatchHandler : IWatchHandler
{
    protected virtual void OnInitialize(object model) { }
    protected virtual void OnRefresh(object model, bool isFirst) { }
}

// ✅ Reusable context base
public abstract class ControlContext(Control control, Action<object?> setValue) : Disposable
{
    protected virtual void OnBindEvent() { }
    protected virtual void OnUnbindEvent() { }
}
```

#### Generic Abstract Base Classes
```csharp
// ✅ Eliminate repetition across attribute-based factories
public abstract class WatchFactoryFromAttribute<TAttribute> : IWatchFactory where TAttribute : Attribute
{
    protected virtual bool CanHandle(PropertyGridItem gridItem, TAttribute[] attributes);
    protected abstract IWatchHandler? OnCreateInstance(PropertyGridItem gridItem, TAttribute[] attributes);
}
```

#### Extension Methods for Reusability
```csharp
// ✅ Common operations as extensions
public static class ControlExtension
{
    public static void CrossThread(this Control control, Action action) { /* ... */ }
}
```

### Design Patterns

#### Factory Pattern (Chain of Responsibility)
```csharp
// ✅ Priority-based factory selection
public interface IControlFactory
{
    int Priority => 0;  // 100+ for attribute-based, 0 for type-based
    bool CanHandle(Metadata property);
    ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged);
}
```

#### Template Method Pattern
```csharp
// ✅ Fixed algorithm with customizable steps
public abstract class WatchHandler
{
    public void Refresh(object model)
    {
        if (!IsInitialized) OnInitialize(model);
        OnRefresh(model, isFirst);
    }
    
    protected virtual void OnInitialize(object model) { }
    protected virtual void OnRefresh(object model, bool isFirst) { }
}
```

#### Observer Pattern
```csharp
// ✅ Reactive programming with Subject<T>
public Subject<ModelPropertyChanged> PropertyChanged { get; }
```

#### Strategy Pattern
```csharp
// ✅ Interchangeable algorithms
public interface ITransitionType { /* ... */ }
public class TransitionLinear(int duration) : Animation(duration) { }
public class TransitionBounce : TransitionUserDefined { }
```

## Naming Conventions

### Fields & Properties
```csharp
// ✅ Private fields - PascalCase (not camelCase, not _fieldName)
private readonly List<IControlFactory> Factories;
private readonly ErrorProvider ErrorProvider;
private readonly Lock Sync = new();

// ✅ Private backing fields for properties - PascalCase with 'P' prefix
private ThemeType PTheme;
public ThemeType ThemeType {
    get => PTheme;
    set { PTheme = value; }
}

// ✅ Public properties - PascalCase
public Rectangle Bounds { get; }
public bool IsChecked { get; private set; }

// ✅ Protected properties - PascalCase
protected Metadata Checked { get; private set; }
```

### Methods
```csharp
// ✅ All methods - PascalCase (public, protected, private)
public void BindModel(Control container, object model) { }
protected virtual void OnInitialize(object model) { }
private void UpdateSize() { }

// ✅ Template method hooks - On prefix
protected override void OnBindEvent() { }
protected override void OnRefresh(object model, bool isFirst) { }
protected override void OnDispose() { }
```

### Classes & Interfaces
```csharp
// ✅ Interfaces - I prefix, PascalCase
public interface IControlFactory { }
public interface IWatchHandler { }

// ✅ Classes - PascalCase
public class StringControlFactory : IControlFactory { }
public class EnabledWhenHandler : WatchFactoryFromAttribute<EnabledWhenAttribute> { }

// ✅ Sealed attributes - sealed keyword
public sealed class EnabledWhenAttribute : WhenAttribute { }

// ✅ Abstract base classes - abstract keyword
public abstract class WatchHandler : IWatchHandler { }
public abstract class ControlContext : Disposable { }

// ✅ Nested private classes - descriptive names
private class EnabledWhenHandlerInstance : WhenHandler { }
private sealed class FlickerFreePanel : Control { }
```

### Parameters & Locals
```csharp
// ✅ Parameters - camelCase
public ControlContext(Control control, Action<object?> setValue) { }
public void Refresh(object model, bool isFirst) { }

// ✅ Local variables - camelCase
var textBox = new TextBox();
var gridItem = CreatePropertyGridItem(property, model);
```

### Test Classes
```csharp
// ✅ Test class naming - ClassNameTests suffix
public class StringControlFactoryTests { }
public class EnabledWhenHandlerTests { }

// ✅ Test method naming - MethodName_Scenario_ExpectedBehavior
[Test]
public void CanHandle_StringProperty_ReturnsTrue() { }

[Test]
public void CreateControl_WithValidModel_BindsSuccessfully() { }
```

## Code Organization

### Project Structure
```
src/CoreOne.Winforms/
├── Attributes/          # Declarative metadata attributes
├── Controls/            # Custom WinForms controls
├── Events/              # Event argument classes
├── Extensions/          # Extension methods
├── Forms/               # Base form classes
├── Models/              # Data transfer objects, contexts
├── Native/              # P/Invoke and Windows API wrappers
├── Services/            # Business logic and services
│   ├── ControlFactories/  # Control creation factories
│   └── WatchHandlers/     # Reactive property watchers
├── Transitions/         # Animation system
│   ├── Animations/        # Animation implementations
│   └── ManagedType/       # Type-specific animation handlers
└── I*.cs               # Interface definitions at root

Tests/
├── Attributes/          # Mirror src structure
├── Controls/
├── Extensions/
├── Models/
├── Services/
│   ├── ControlFactories/
│   └── WatchHandlers/
└── Integration/         # Integration tests
```

### File Organization Rules
- One primary public class per file (nested private classes allowed)
- File name matches the primary class name
- Test files mirror production structure with "Tests" suffix
- Interfaces at root or in relevant subfolder
- Related implementations grouped in subfolders

## Coding Conventions & Patterns

### Constructor Patterns
```csharp
// ✅ Primary constructor with dependency injection
public class ModelBinder(IServiceProvider services, IRefreshManager refreshManager, IGridLayoutManager layoutManager)
    : Disposable, IModelBinder, IDisposable
{
    private readonly List<IControlFactory> Factories = [.. services.GetRequiredService<IEnumerable<IControlFactory>>()];
}

// ✅ Primary constructor with parameters
public sealed class EnabledWhenAttribute(string propertyName, object? expectedValue, ComparisonType comparisonType = ComparisonType.EqualTo)
    : WhenAttribute(propertyName, expectedValue, comparisonType)
{
}

// ✅ Abstract classes with constructors
public abstract class WatchHandler(Metadata property) : IWatchHandler
{
    public Metadata Property { get; } = property;
}
```

### Disposal Pattern
```csharp
// ✅ Inherit from Disposable base class
public class ModelBinder : Disposable, IModelBinder, IDisposable
{
    protected override void OnDispose()
    {
        // Cleanup logic
        GridItems.Each(p => p.Dispose());
        Transaction?.Dispose();
    }
}

// ✅ IDisposable for cleanup
public abstract class ControlContext : Disposable
{
    protected override void OnDispose()
    {
        OnUnbindEvent();
        Control.Dispose();
    }
}
```

### Null Safety & Validation
```csharp
// ✅ ArgumentNullException.ThrowIfNull for parameter validation
public Size BindModel(Control container, object model)
{
    ArgumentNullException.ThrowIfNull(model);
    ArgumentNullException.ThrowIfNull(container);
    // ...
}

// ✅ Nullable reference types enabled
#nullable enable
public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)

// ✅ Null-conditional and null-coalescing operators
var color = Control?.BackColor ?? ToolStripItem?.BackColor ?? throw new NullReferenceException();
```

### Collection Initialization
```csharp
// ✅ Collection expressions (C# 12)
private readonly List<PropertyGridItem> GridItems = [];
private readonly List<IControlFactory> Factories = [.. services.GetRequiredService<IEnumerable<IControlFactory>>()];

// ✅ Collection initializers
var transitions = new Dictionary<Transition, bool>(50);
```

### LINQ & Functional Patterns
```csharp
// ✅ Method chaining with LINQ
var factories = Factories.OrderByDescending(f => f.Priority).ToList();
var properties = MetaType.GetMetadatas(model.GetType())
    .Where(p => p.CanRead && p.CanWrite)
    .Where(p => p.GetCustomAttribute<IgnoreAttribute>() is null)
    .ToList();

// ✅ Extension methods for fluent API
properties.Each(p => DoSomething(p));
attributes.SelectMany(p => p.PropertyNames).Each(p => Dependencies.Add(p));
```

### Async/Await Patterns
```csharp
// ✅ ValueTask for performance
public virtual ValueTask Initialize(CancellationToken cancellationToken) => ValueTask.CompletedTask;

// ✅ Async with proper cancellation
public async ValueTask Initialize()
{
    var tasks = services.SelectArray(p => callback(p, cancellationToken).AsTask());
    await Task.WhenAll(tasks);
}

// ✅ ConfigureAwait for library code avoided (removed in modern .NET)
await SomeMethodAsync(); // No ConfigureAwait needed
```

### Event Handling
```csharp
// ✅ Event subscription/unsubscription
protected override void OnBindEvent()
{
    control.TextChanged += OnTextChanged;
}

protected override void OnUnbindEvent()
{
    control.TextChanged -= OnTextChanged;
}

// ✅ Reactive programming with Subject<T>
public Subject<ModelPropertyChanged> PropertyChanged { get; } = new();
PropertyChanged.OnNext(new ModelPropertyChanged(property.Name, current, value));
```

### Cross-Thread UI Updates
```csharp
// ✅ Safe cross-thread control updates
control.CrossThread(() => {
    control.Text = "Updated";
    control.Invalidate();
});

// ✅ Implementation pattern
public static void CrossThread(this Control control, Action action)
{
    if (control.InvokeRequired)
        control.Invoke(action);
    else
        action();
}
```

### Nested Implementation Classes
```csharp
// ✅ Private nested classes for implementations
public class EnabledWhenHandler : WatchFactoryFromAttribute<EnabledWhenAttribute>
{
    private class EnabledWhenHandlerInstance(PropertyGridItem gridItem, EnabledWhenAttribute[] attributes)
        : WhenHandler(gridItem, attributes)
    {
        protected override void OnCondiition(object model, IReadOnlyList<bool> flags)
        {
            var shouldEnable = flags.All(f => f);
            PropertyGridItem.InputControl.Enabled = shouldEnable;
        }
    }
    
    protected override IWatchHandler? OnCreateInstance(PropertyGridItem gridItem, EnabledWhenAttribute[] attributes)
        => new EnabledWhenHandlerInstance(gridItem, attributes);
}
```

### Attribute Usage
```csharp
// ✅ Sealed attributes (leaf nodes in hierarchy)
public sealed class EnabledWhenAttribute : WhenAttribute { }
public sealed class FileAttribute : Attribute { }

// ✅ Abstract base attributes (for extension)
public abstract class WhenAttribute : Attribute { }

// ✅ AttributeUsage specification
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class EnabledWhenAttribute : WhenAttribute { }

// ✅ Primary constructor for attributes
public sealed class FileAttribute(string filter = "All Files (*.*)|*.*", bool multiselect = false) : Attribute
{
    public string Filter { get; } = filter;
    public bool Multiselect { get; } = multiselect;
}
```

### Documentation
```csharp
// ✅ XML documentation for public APIs
/// <summary>
/// Factory interface for creating WinForms controls based on property metadata and attributes.
/// Factories use a priority-based chain of responsibility pattern.
/// </summary>
/// <remarks>
/// <para><strong>Priority System:</strong></para>
/// <list type="bullet">
/// <item><description><strong>100+</strong>: Attribute-based factories</description></item>
/// <item><description><strong>0</strong>: Generic type-based factories</description></item>
/// </list>
/// </remarks>
public interface IControlFactory

// ✅ Code examples in documentation
/// <example>
/// <code>
/// [EnabledWhen(nameof(IsActive), true)]
/// public string Notes { get; set; }
/// </code>
/// </example>
```

## Testing Guidelines

### Test File Structure & Naming
```csharp
// ✅ No [TestFixture] attribute - public class without attributes
// ✅ Namespace follows Tests.{Namespace} pattern
// ✅ File name matches class under test with "Tests" suffix
namespace Tests.Extensions;

public class EnumerableExtensionsTests  // Not EnumerableExtensionsTestFixture
{
    // Tests here
}
```

### Test Method Patterns
```csharp
// ✅ Use [Test] attribute for test methods
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var list = new List<int> { 1, 2, 3 };
    
    // Act
    var result = list.AddOrUpdate(4);
    
    // Assert
    Assert.That(result, Has.Count.EqualTo(4));
}

// ✅ Use [TestCase] for parameterized tests
[TestCase(new[] { 0, 1, 2 })]
public void Each_WithIndex_ProvidesIndexToAction(int[] data)
{
    var list = new List<string> { "a", "b", "c" };
    var indices = new List<int>();
    list.Each((item, index) => indices.Add(index));
    Assert.That(indices, Is.EqualTo(data));
}

// ✅ Use [SetUp] for common initialization
[SetUp]
public void Setup()
{
    // Common setup code
}

// ✅ Use [TearDown] for cleanup
[TearDown]
public void TearDown()
{
    hub?.Dispose();
}
```

### Assertion Patterns (NUnit)
```csharp
// ✅ NUnit fluent syntax with Assert.That
Assert.That(result, Is.EqualTo(expected));
Assert.That(collection, Has.Count.EqualTo(3));
Assert.That(collection, Does.Contain(item));
Assert.That(collection, Does.Not.Contain(item));
Assert.That(collection, Is.Empty);
Assert.That(collection, Is.Not.Null);
Assert.That(result, Is.True);
Assert.That(result, Is.False);
Assert.That(result, Is.SameAs(expected));

// ✅ Multiple assertions grouped together
using (Assert.EnterMultipleScope())
{
    Assert.That(result, Is.True);
    Assert.That(set, Has.Count.EqualTo(1));
}

// ✅ Collection initialization in assertions
var set = new ConcurrentSet<int> {
    1,
    2,
    3
};

// ❌ Avoid - Old assertion style
Assert.AreEqual(expected, actual);  // Use Assert.That instead
```

### Async Test Patterns
```csharp
// ✅ Use TaskCompletionSource for synchronization
[Test]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    var tcs = new TaskCompletionSource<IMessage?>(TaskCreationOptions.RunContinuationsAsynchronously);
    var mock = new Mock<Action<IMessage>>();
    mock.Setup(m => m(It.IsAny<IMessage>())).Callback<IMessage>(m => tcs.TrySetResult(m));
    
    hub.Subscribe<IMessage>(msg => { mock.Object(msg); return Task.CompletedTask; }, null, CancellationToken.None);
    hub.Publish(msg);
    
    var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(2)));
    Assert.That(completed, Is.EqualTo(tcs.Task));
}

// ✅ Use Task.Delay for timing tests
await Task.Delay(100);  // Wait for debounce delay
Assert.That(executed, Is.True);

// ✅ Use CancellationToken for cancellable operations
var cts = new CancellationTokenSource();
hub.Subscribe<IMessage>(HandleMessage, null, cts.Token);
cts.Cancel();  // Unsubscribe
```

### Mocking with Moq
```csharp
// ✅ Mock setup with callback
var mock = new Mock<Action<IMessage>>();
mock.Setup(m => m(It.IsAny<IMessage>()))
    .Callback<IMessage>(m => tcs.TrySetResult(m));

// ✅ Verify method invocation
mock.Verify(m => m(It.IsAny<IMessage>()), Times.Once);
mock.Verify(m => m(It.IsAny<IMessage>()), Times.Never());

// ✅ Mock services for dependency injection
var mockLogger = new Mock<ILogger>();
var services = new ServiceCollection()
    .AddSingleton<ITestService, TestServiceImpl>()
    .AddSingleton<ILogger>(mockLogger.Object)
    .BuildServiceProvider();

// ⚠️ Mock types must be public (Moq limitation)
public interface IMessage { }  // Not private
public class MessageImpl : IMessage { }  // Not private
```

### Test Data & Helper Classes
```csharp
// ✅ Define test-specific types within test class
public class EnumerableExtensionsTests
{
    // Helper classes for testing
    private class TestModel
    {
        public string Name { get; set; } = "Test";
        public int Value { get; set; }
    }
    
    // Tests use helper classes
    [Test]
    public void Test_WithHelperClass()
    {
        var model = new TestModel { Value = 42 };
        // ...
    }
}

// ✅ For tests requiring public types (Moq), define at namespace level
namespace Tests;

public class HubTests
{
    // Must be public for Moq
    public interface IMessage { int Value { get; } }
    public class MessageImpl : IMessage { public int Value { get; set; } }
}
```

### Null Handling Tests
```csharp
// ✅ Always test null scenarios
[Test]
public void Method_WithNull_ReturnsEmpty()
{
    List<string>? list = null;
    var result = list.ExcludeNulls();
    Assert.That(result, Is.Empty);
}

[Test]
public void Method_NullParameter_HandlesGracefully()
{
    Assert.DoesNotThrow(() => ServiceInitializer.Initialize(instance, null));
}
```

### Edge Case & Boundary Testing
```csharp
// ✅ Test empty collections
[Test]
public void Method_WithEmptyCollection_ReturnsExpected()
{
    var list = new List<int>();
    var result = list.ExcludeNulls();
    Assert.That(result, Is.Empty);
}

// ✅ Test boundary values
[Test]
public void Debounce_ZeroDelay_ExecutesImmediately()
{
    var debounce = new Debounce(() => executed = true, TimeSpan.Zero);
    debounce.Invoke();
    await Task.Delay(10);
    Assert.That(executed, Is.True);
}

// ✅ Test duplicate handling
[Test]
public void Add_DuplicateItem_ReturnsFalse()
{
    var set = new ConcurrentSet<int> { 1 };
    var result = set.Add(1);
    Assert.That(result, Is.False);
}
```

### Thread Safety Testing
```csharp
// ✅ Test concurrent operations
[Test]
public async Task ConcurrentSet_ThreadSafety_HandlesRaceConditions()
{
    var set = new ConcurrentSet<int>();
    var tasks = Enumerable.Range(0, 100)
        .Select(i => Task.Run(() => set.Add(i)));
    
    await Task.WhenAll(tasks);
    
    Assert.That(set, Has.Count.EqualTo(100));
}
```

### Validation & Error Testing
```csharp
// ✅ Test validation success
[Test]
public void ValidateModel_ValidObject_ReturnsSuccess()
{
    var model = new ValidModel();
    var result = model.ValidateModel(null, false);
    Assert.That(result.IsValid, Is.True);
}

// ✅ Test validation failure
[Test]
public void ValidateModel_InvalidObject_ReturnsFail()
{
    var model = new InvalidModel();
    var result = model.ValidateModel(null, false);
    using (Assert.EnterMultipleScope())
    {
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessages, Is.Not.Empty);
    }
}
```

### Test Organization & Coverage
- **One test class per production class**: `HubTests.cs` tests [Hub.cs](../src/CoreOne/Hubs/Hub.cs)
- **Descriptive test names**: Use `MethodName_Scenario_ExpectedBehavior` pattern
- **Test file location**: Mirror production namespace under `Tests/` folder
  - `Tests/Extensions/EnumerableExtensionsTests.cs` tests `CoreOne/Extensions/EnumerableExtensions.cs`
  - `Tests/Collections/DataTests.cs` tests `CoreOne/Collections/Data.cs`
- **Comprehensive coverage**: Test happy path, edge cases, null handling, error conditions, async behavior
- **Maintainability**: Keep tests focused, independent, and fast

### Common Test Scenarios to Cover
1. **Happy path** - Normal expected usage
2. **Null handling** - Null parameters, null collections
3. **Empty collections** - Empty lists, empty strings
4. **Edge cases** - Zero values, max values, boundary conditions
5. **Error conditions** - Invalid input, exceptions
6. **Async behavior** - Delays, cancellation, race conditions
7. **Thread safety** - Concurrent operations (for concurrent types)
8. **Inheritance/polymorphism** - Base class scenarios, interface implementations
