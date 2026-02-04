
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
