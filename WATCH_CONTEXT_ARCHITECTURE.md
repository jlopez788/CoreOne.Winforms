# WatchContext Architecture

## Overview

The WatchContext system provides a **plugin-based, extensible architecture** for handling property change notifications and executing reactive behaviors in WinForms applications. The design follows the **pipeline pattern** where handlers can be registered and automatically applied based on attributes and context.

## Key Components

### 1. **IWatchContextHandler** - The Plugin Interface
Defines how handlers can determine if they're applicable and create handler instances.

```csharp
public interface IWatchContextHandler
{
    bool CanHandle(Metadata property, object? context = null);
    IWatchContextHandlerInstance CreateInstance(Metadata property, object? context);
}
```

### 2. **IWatchContextHandlerInstance** - The Execution Instance
Represents an instance that processes events for a specific property.

```csharp
public interface IWatchContextHandlerInstance
{
    IEnumerable<string> Dependencies { get; }
    void Initialize(object model);
    void Refresh(object model);
}
```

### 3. **WatchContextHandlerRegistry** - The Plugin Registry
Centralized singleton registry where handlers are registered.

```csharp
var registry = WatchContextHandlerRegistry.Instance;
registry.Register(new MyCustomHandler());
```

### 4. **WatchContext** - The Pipeline Orchestrator
Manages a collection of handler instances and orchestrates their execution.

```csharp
var context = WatchContext.Create(property, controlContext);
context.RequestRefresh(model);
```

## Built-in Handlers

### EnabledWhenHandler
Controls UI element enabled state based on property values and comparison operators.

**Attribute:** `EnabledWhenAttribute`

**Context:** `Control`

**Example:**
```csharp
[EnabledWhen(nameof(IsAdmin), true)]
public string AdminAction { get; set; }
```

### ComputeHandler
Executes a method to compute property values dynamically.

**Attribute:** `ComputeAttribute`

**Context:** `PropertyGridItem`

**Example:**
```csharp
[Compute("CalculateTotal")]
[WatchProperties(nameof(Quantity), nameof(Price))]
public decimal Total { get; set; }

public decimal CalculateTotal(decimal quantity, decimal price)
{
    return quantity * price;
}
```

### DropdownSourceHandler
Refreshes dropdown items based on property changes.

**Attribute:** `WatchPropertiesAttribute` (or inherits from it)

**Context:** `IDropdownSourceProvider`

**Example:**
```csharp
[DropdownSource(typeof(CityDropdownProvider))]
[WatchProperties(nameof(CountryId))]
public int CityId { get; set; }
```

## Creating Custom Handlers

### Step 1: Implement IWatchContextHandler

```csharp
public class VisibilityHandler : IWatchContextHandler
{
    public bool CanHandle(Metadata property, object? context = null)
    {
        return context is Control && 
               property.GetCustomAttribute<VisibleWhenAttribute>() != null;
    }

    public IWatchContextHandlerInstance CreateInstance(Metadata property, object? context)
    {
        var control = (Control)context!;
        var attribute = property.GetCustomAttribute<VisibleWhenAttribute>()!;
        return new VisibilityHandlerInstance(property, control, attribute);
    }
}
```

### Step 2: Implement Handler Instance

```csharp
private class VisibilityHandlerInstance : WatchContextHandlerInstanceBase
{
    private readonly Control _control;
    private readonly VisibleWhenAttribute _attribute;
    private Metadata _targetProperty;

    public VisibilityHandlerInstance(Metadata property, Control control, 
        VisibleWhenAttribute attribute) : base(property, control)
    {
        _control = control;
        _attribute = attribute;
    }

    public override IEnumerable<string> Dependencies => 
        new[] { _attribute.PropertyName };

    protected override void OnInitialize(object model)
    {
        _targetProperty = MetaType.GetMetadata(
            model.GetType(), _attribute.PropertyName);
    }

    protected override void OnRefresh(object model)
    {
        var value = _targetProperty.GetValue(model);
        var isVisible = Equals(value, _attribute.ExpectedValue);
        
        _control.CrossThread(() => _control.Visible = isVisible);
    }
}
```

### Step 3: Register the Handler

```csharp
// During application startup
WatchContextHandlerRegistry.Instance.Register(new VisibilityHandler());
```

Or add to the default registration:

```csharp
public static class WatchContextHandlers
{
    public static void RegisterDefaults()
    {
        var registry = WatchContextHandlerRegistry.Instance;
        
        // Built-in handlers
        registry.Register(new EnabledWhenHandler());
        registry.Register(new ComputeHandler());
        registry.Register(new DropdownSourceHandler());
        
        // Custom handlers
        registry.Register(new VisibilityHandler());
        registry.Register(new ValidationHandler());
    }
}
```

### Step 4: Use Your Handler

```csharp
public class MyModel
{
    public bool IsAdvancedMode { get; set; }
    
    [VisibleWhen(nameof(IsAdvancedMode), true)]
    public string AdvancedSetting { get; set; }
}
```

## Usage Patterns

### Pattern 1: Automatic Handler Discovery (Recommended)

```csharp
// Initialize handlers once at application startup
WatchContextHandlers.RegisterDefaults();

// Create context with automatic handler discovery
var watchContext = WatchContext.Create(propertyMetadata, control);
watchContext.RequestRefresh(model);
```

### Pattern 2: Manual Handler Registration

```csharp
var watchContext = new WatchContext(propertyMetadata);
watchContext.AddHandler(new EnabledWhenHandlerInstance(...));
watchContext.AddHandler(new ComputeHandlerInstance(...));
watchContext.RequestRefresh(model);
```

### Pattern 3: Legacy Support

```csharp
// Old code continues to work
var context = new EnabledContext(control, property, attribute);
var context = new ComputeContext(gridItem, property, attribute);
```

## Benefits of This Architecture

### 1. **Extensibility**
- Add new handlers without modifying existing code
- Each handler is independent and focused on a single responsibility

### 2. **Determinism**
- Clear registration and discovery mechanism
- Predictable execution order based on registration

### 3. **Testability**
- Handlers can be tested in isolation
- Registry can be cleared and mocked for testing

### 4. **Separation of Concerns**
- Handler logic separated from context orchestration
- Each handler focuses on one type of behavior

### 5. **Backward Compatibility**
- Existing code continues to work
- Gradual migration path to new architecture

### 6. **Plug-and-Play**
- Register handlers at runtime
- Enable/disable features by registering/unregistering handlers

## Future Extensions

### Example: Validation Handler

```csharp
public class ValidationHandler : IWatchContextHandler
{
    public bool CanHandle(Metadata property, object? context = null)
    {
        return property.GetCustomAttributes<ValidationAttribute>().Any();
    }

    public IWatchContextHandlerInstance CreateInstance(
        Metadata property, object? context)
    {
        var validationAttributes = property
            .GetCustomAttributes<ValidationAttribute>().ToList();
        return new ValidationHandlerInstance(property, validationAttributes);
    }
}
```

### Example: Async Data Loading Handler

```csharp
public class AsyncDataLoadHandler : IWatchContextHandler
{
    public bool CanHandle(Metadata property, object? context = null)
    {
        return property.GetCustomAttribute<AsyncDataSourceAttribute>() != null;
    }

    public IWatchContextHandlerInstance CreateInstance(
        Metadata property, object? context)
    {
        var attribute = property.GetCustomAttribute<AsyncDataSourceAttribute>();
        return new AsyncDataLoadHandlerInstance(property, attribute);
    }
}
```

### Example: Conditional Styling Handler

```csharp
public class ConditionalStyleHandler : IWatchContextHandler
{
    public bool CanHandle(Metadata property, object? context = null)
    {
        return context is Control && 
               property.GetCustomAttribute<StyleWhenAttribute>() != null;
    }

    public IWatchContextHandlerInstance CreateInstance(
        Metadata property, object? context)
    {
        return new ConditionalStyleHandlerInstance(
            property, (Control)context, 
            property.GetCustomAttribute<StyleWhenAttribute>());
    }
}
```

## Migration Guide

### Before (Old Architecture)
```csharp
// Tightly coupled to specific implementations
var enabledContext = new EnabledContext(control, property, attr);
var computeContext = new ComputeContext(gridItem, property, attr);
```

### After (New Architecture)
```csharp
// Register handlers once during startup
WatchContextHandlers.RegisterDefaults();

// Use unified creation method
var watchContext = WatchContext.Create(property, control);
// Handlers are automatically discovered and registered
```

### Adding New Feature (Old Way)
1. Create a new `XyzContext` class inheriting from `WatchContext`
2. Override `OnInitialize` and `OnRefresh`
3. Update all calling code to use new context class
4. Handle dependencies manually

### Adding New Feature (New Way)
1. Create `XyzHandler : IWatchContextHandler`
2. Register it: `registry.Register(new XyzHandler())`
3. Done! Existing code automatically picks it up

## Performance Considerations

- Handler registration is done once at startup (minimal overhead)
- Handler discovery uses `CanHandle()` which should be lightweight
- Handler instances are created per property (not per model instance)
- Lock-based synchronization ensures thread safety without excessive locking

## Best Practices

1. **Keep handlers focused** - One handler = One responsibility
2. **Make CanHandle() efficient** - This is called frequently
3. **Register handlers early** - During application startup
4. **Use base classes** - Inherit from `WatchContextHandlerInstanceBase`
5. **Handle errors gracefully** - Don't let one handler break others
6. **Document dependencies** - Make clear what properties are watched
7. **Test in isolation** - Each handler should have unit tests
