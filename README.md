# CoreOne.Winforms

A modern, feature-rich WinForms library that provides **dynamic model-based form generation** with automatic two-way data binding, extensible handler pipeline, themed controls, and smooth animations. Built for .NET 9.0 with dependency injection support.

[![NuGet](https://img.shields.io/nuget/v/CoreOne.Winforms.svg)](https://www.nuget.org/packages/CoreOne.Winforms)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 🚀 Features

### Dynamic Form Generation
- **ModelControl** - Automatically generates form fields from your model properties
- **Two-way data binding** - Changes in controls update the model, and vice versa
- **Reactive updates** - Built on reactive patterns with `Subject<T>` for change notifications
- **Dirty tracking** - Know when data has been modified with `IsDirty` flag

### Smart Grid Layout
- **6-column Bootstrap-style grid** - Responsive layout system
- **`[GridColumn]` attribute** - Control field width (1-6 columns)
- **Auto-sizing** - Calculates ideal form dimensions
- **Labels above controls** - Clean, modern layout

### Dropdown Support
- **Attribute-based providers** - `[DropdownSource(typeof(Provider))]` attribute
- **Dependency tracking** - `[WatchProperties]` for cascading dropdowns
- **Sync & Async providers** - Support both `IDropdownSourceProviderSync` and `IDropdownSourceProviderAsync`
- **Auto-refresh** - Dependent dropdowns update automatically when watched properties change

### Extensible Handler Pipeline
- **Plug-and-play architecture** - Register custom handlers for reactive behaviors
- **Built-in handlers** - EnabledWhen, Compute, and Dropdown handlers included
- **IWatchFactory interface** - Create custom handlers that automatically integrate
- **Priority-based execution** - Control handler execution order
- **Attribute-driven** - Handlers activate based on property attributes

### Themed Controls
- **OButton** - Modern button with hover/press states
- **AnimatedPanel** - Smooth transitions and animations
- **LoadingCircle** - Loading indicator control
- **RatingControl** - Star rating input
- **BaseView** - Base control for creating custom views
- **OneForm** - Enhanced form with async support and loading indication

### Advanced Features
- **Computed Properties** - `[Compute("MethodName")]` executes methods to calculate values
- **Conditional Enabling** - `[EnabledWhen]` for dynamic control state
- **Conditional Visibility** - `[VisibleWhen]` for dynamic control visibility
- **Conditional Clearing** - `[ClearWhen]` to clear values based on conditions
- **Property Watching** - `[WatchProperty]` and `[WatchProperties]` for reactive monitoring
- **File Selection** - `[File(filter, multiselect)]` for file/folder pickers
- **Validation Support** - Built-in validation with error providers
- **Change Management** - `RejectChanges()` and `AcceptChanges()` for undo/redo
- **Dependency Injection** - First-class DI support with `IServiceProvider`
- **SOLID Architecture** - Clean codebase following SOLID & DRY principles

## 📦 Installation

Install via NuGet Package Manager:

```bash
dotnet add package CoreOne.Winforms
```

Or via Package Manager Console:

```powershell
Install-Package CoreOne.Winforms
```

## 🎯 Quick Start

### 1. Define Your Model

```csharp
using CoreOne.Winforms.Attributes;

public class Customer
{
    [GridColumn(GridColumnSpan.Half)]  // Takes 3 columns (half of 6)
    public string FirstName { get; set; }
    
    [GridColumn(GridColumnSpan.Half)]
    public string LastName { get; set; }
    
    [GridColumn(GridColumnSpan.Full)]  // Takes all 6 columns
    public string Email { get; set; }
    
    [DropdownSource(typeof(CountryProvider))]
    [GridColumn(GridColumnSpan.Half)]
    public string Country { get; set; }
    
    [DropdownSource(typeof(StateProvider))]
    [WatchProperty(nameof(Country))]  // Refreshes when Country changes
    [GridColumn(GridColumnSpan.Half)]
    public string State { get; set; }
    
    public DateTime BirthDate { get; set; }
    
    public bool IsActive { get; set; }
    
    [EnabledWhen(nameof(IsActive), true)]  // Only enabled when IsActive is true
    [ClearWhen(nameof(IsActive), false)]  // Clears value when IsActive becomes false
    public string ActiveNotes { get; set; }
    
    [Rating(5)]  // Display as 5-star rating control
    public int CustomerRating { get; set; }
    
    [Compute("CalculateTotalScore")]
    [WatchProperties(nameof(CustomerRating), nameof(IsActive))]
    public decimal TotalScore { get; set; }
    
    [File("All Files (*.*)|*.*")]  // File picker control
    [VisibleWhen(nameof(IsActive), true)]  // Only visible when IsActive is true
    public string? AttachmentPath { get; set; }
    
    [Ignore]  // Won't generate a control
    public int InternalId { get; set; }
    
    // Computed property method
    public decimal CalculateTotalScore(int customerRating, bool isActive)
    {
        return isActive ? customerRating * 10 : 0;
    }
}
```

### 2. Setup Dependency Injection

```csharp
using CoreOne.Winforms.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register CoreOne.Winforms services
services.AddFormServices();

// Register your dropdown providers
services.AddSingleton<CountryProvider>();
services.AddSingleton<StateProvider>();

var serviceProvider = services.BuildServiceProvider();
```

### 3. Create and Use ModelControl

```csharp
using CoreOne.Winforms.Controls;

public class MyForm : Form
{
    private readonly ModelControl _modelControl;
    
    public MyForm(IServiceProvider services)
    {
        var modelBinder = services.GetRequiredService<IModelBinder>();
        _modelControl = new ModelControl(services, modelBinder);
        _modelControl.Dock = DockStyle.Fill;
        
        // Handle save button click
        _modelControl.SaveClicked += OnSaveClicked;
        
        Controls.Add(_modelControl);
        
        // Bind your model
        var customer = new Customer 
        { 
            FirstName = "John", 
            LastName = "Doe" 
        };
        _modelControl.SetModel(customer);
    }
    
    private void OnSaveClicked(object? sender, ModelSavedEventArgs e)
    {
        if (e.IsModified)
        {
            // Validation is automatically performed
            if (!e.IsValid)
            {
                MessageBox.Show("Please fix validation errors");
                return;
            }
            
            // Get the updated model
            var customer = _modelControl.GetModel<Customer>();
            
            // Save to database...
            MessageBox.Show($"Saved: {customer?.FirstName} {customer?.LastName}");
            
            _modelControl.AcceptChanges();  // Commit changes and clear dirty flag
        }
    }
    
    // Optional: Reject changes and revert to original values
    private void OnCancelClicked(object? sender, EventArgs e)
    {
        _modelControl.RejectChanges();  // Rollback all changes
    }
}
```

## 📚 Advanced Usage

### Creating Custom Dropdown Providers

**Synchronous Provider:**

```csharp
using CoreOne.Winforms;
using CoreOne.Winforms.Models;

public class CountryProvider : IDropdownSourceProviderSync
{
    public void Initialize(IWatchHandler context) { }
    
    public IEnumerable<DropdownItem> GetItems(object model)
    {
        return
        [
            new DropdownItem("US", "United States"),
            new DropdownItem("CA", "Canada"),
            new DropdownItem("MX", "Mexico")
        ];
    }
    
    public void Dispose() { }
}
```

**Dependent (Cascading) Provider:**

```csharp
public class StateProvider : IDropdownSourceProviderSync
{
    public void Initialize(IWatchHandler context) { }
    
    public IEnumerable<DropdownItem> GetItems(object model)
    {
        var customer = (Customer)model;
        
        return customer.Country switch
        {
            "US" => 
            [
                new DropdownItem("CA", "California"),
                new DropdownItem("TX", "Texas"),
                new DropdownItem("NY", "New York")
            ],
            "CA" => 
            [
                new DropdownItem("ON", "Ontario"),
                new DropdownItem("BC", "British Columbia")
            ],
            _ => []
        };
    }
    
    public void Dispose() { }
}
```

**Async Provider (for API calls):**

```csharp
public class ProductCategoriesProvider(HttpClient httpClient) : IDropdownSourceProviderAsync
{
    public void Initialize(IWatchHandler context) { }
    
    public async Task<IEnumerable<DropdownItem>> GetItemsAsync(object model)
    {
        var categories = await httpClient.GetFromJsonAsync<Category[]>("/api/categories");
        return categories?.Select(c => new DropdownItem(c.Id.ToString(), c.Name)) ?? [];
    }
    
    public void Dispose() => httpClient?.Dispose();
}
```

### Creating Custom Watch Handlers

Extend the library with custom reactive handlers using the `WatchFactoryFromAttribute<TAttribute>` base class:

```csharp
using CoreOne.Winforms;
using CoreOne.Winforms.Services.WatchHandlers;

// 1. Create your custom attribute
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class CustomWhenAttribute(string propertyName, object? expectedValue) 
    : WhenAttribute(propertyName, expectedValue)
{
}

// 2. Create the handler factory
public class CustomWhenHandler : WatchFactoryFromAttribute<CustomWhenAttribute>
{
    // Private nested implementation class
    private class CustomWhenHandlerInstance(PropertyGridItem gridItem, CustomWhenAttribute[] attributes)
        : WhenHandler(gridItem, attributes)
    {
        protected override void OnCondition(object model, IReadOnlyList<bool> flags)
        {
            // Custom logic when condition changes
            var shouldApply = flags.All(f => f);
            PropertyGridItem.InputControl.CrossThread(() => 
            {
                PropertyGridItem.InputControl.BackColor = shouldApply 
                    ? Color.LightGreen 
                    : Color.White;
            });
        }
    }
    
    protected override IWatchHandler? OnCreateInstance(
        PropertyGridItem gridItem, 
        CustomWhenAttribute[] attributes)
    {
        return new CustomWhenHandlerInstance(gridItem, attributes);
    }
}

// 3. Register (auto-registered via AddFormServices if in same assembly)
// Manual: services.AddSingleton<IWatchFactory, CustomWhenHandler>();

// 4. Use it
public class MyModel
{
    public bool IsActive { get; set; }
    
    [CustomWhen(nameof(IsActive), true)]
    public string ActiveField { get; set; }
}
```

### Monitoring Property Changes

```csharp
// Subscribe to property changes
var subscription = _modelControl.PropertyChanged?.Subscribe(change =>
{
    Console.WriteLine($"Property '{change.PropertyName}' changed " +
                     $"from '{change.OldValue}' to '{change.NewValue}'");
});

// Later, dispose to unsubscribe
subscription?.Dispose();
```

### Available Attributes

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[GridColumn(GridColumnSpan)]` | Control column span (1-6) | `[GridColumn(GridColumnSpan.Full)]` |
| `[DropdownSource(typeof(Provider))]` | Specify dropdown provider | `[DropdownSource(typeof(CountryProvider))]` |
| `[WatchProperty("prop")]` | Watch single property | `[WatchProperty(nameof(Country))]` |
| `[WatchProperties("prop1", "prop2")]` | Watch multiple properties | `[WatchProperties(nameof(FirstName), nameof(LastName))]` |
| `[EnabledWhen("prop", value, comparison)]` | Conditionally enable control | `[EnabledWhen(nameof(IsActive), true)]` |
| `[VisibleWhen("prop", value, comparison)]` | Conditionally show/hide control | `[VisibleWhen(nameof(ShowAdvanced), true)]` |
| `[ClearWhen("prop", value, comparison)]` | Clear value when condition met | `[ClearWhen(nameof(IsActive), false)]` |
| `[Compute("MethodName")]` | Execute method to compute value | `[Compute("CalculateTotal")]` |
| `[File(filter, multiselect)]` | File/folder picker control | `[File("Text files (*.txt)|*.txt", false)]` |
| `[Rating(maxRating)]` | Star rating control | `[Rating(5)]` |
| `[Ignore]` | Skip property in form generation | `[Ignore]` |
| `[Visible(bool)]` | Control visibility | `[Visible(false)]` |

### Grid Column Spans

```csharp
public enum GridColumnSpan
{
    None = 0,
    One = 1,         // 1/6 width
    Two = 2,         // 1/3 width
    Three = 3,       // 1/2 width (Half width)
    Four = 4,        // 2/3 width
    Five = 5,        // 5/6 width
    Six = 6,         // Full width
    Default = Six    // Default is full width (6 columns)
}
```

## 🎨 Themed Controls

### OButton - Modern Button Control

```csharp
var button = new OButton
{
    Text = "Click Me",
    BackColor = Color.FromArgb(0, 120, 215),
    ForeColor = Color.White,
    Width = 120,
    Height = 35
};
```

### AnimatedPanel - Transitions & Animations

```csharp
var panel = new AnimatedPanel();
// Built-in transition support for smooth animations
```

### LoadingCircle

```csharp
var loading = new LoadingCircle
{
    Active = true,
    Color = Color.Blue
};
```

### RatingControl - Star Rating

```csharp
var rating = new RatingControl
{
    MaxRating = 5,
    Value = 3
};

rating.ValueChanged += (s, e) => 
{
    Console.WriteLine($"Rating changed to: {rating.Value}");
};
```

## 🏗️ Architecture

CoreOne.Winforms follows **SOLID & DRY principles** with a clean, extensible architecture:

### Design Patterns
- **Factory Pattern** - Priority-based chain of responsibility for control/handler creation
- **Template Method Pattern** - `WatchHandler` base class with `OnInitialize`/`OnRefresh` hooks
- **Observer Pattern** - Reactive programming with `Subject<T>` for property changes
- **Strategy Pattern** - Interchangeable animation algorithms (`TransitionLinear`, `TransitionBounce`)

### Core Services
- **IModelBinder** - Two-way binding between models and controls
- **IPropertyGridItemFactory** - Property grid item creation with labels
- **IGridLayoutManager** - 6-column Bootstrap-style responsive layout
- **IRefreshManager** - Property change notification coordination
- **IControlStateManager** - Control state management (Normal/Hover/Pressed)
- **FormContainerHostService** - Hosted form services lifecycle management

### Extensibility Points
- **IControlFactory** - Control creation based on types/attributes
  - **Priority 100+**: Attribute-based (`[Rating]`, `[DropdownSource]`, `[File]`)
  - **Priority 0**: Type-based (String, Numeric, Boolean, DateTime, Enum)
- **IWatchFactory** - Reactive property watch handlers
  - `EnabledWhenHandler` - Conditional enabling
  - `VisibleWhenHandler` - Conditional visibility
  - `ClearWhenHandler` - Conditional value clearing
  - `ComputeHandler` - Computed properties
  - `DropdownHandler` - Cascading dropdown refresh
- **Dependency Injection** - Constructor injection with `IServiceProvider`
- **Reactive Programming** - Observable changes with `Subject<T>`

### Factory Priority System

Factories use a priority-based chain of responsibility to determine property handling:
- **Priority 100+**: Attribute-based factories (e.g., `[Rating]`, `[File]`, `[DropdownSource]`)
- **Priority 0**: Type-based factories (e.g., String, Numeric, Boolean, DateTime, Enum)

**Custom Factory Example:**
```csharp
public class MyCustomFactory : IControlFactory
{
    public int Priority => 50;  // Medium priority (checked before type-based)
    
    public bool CanHandle(Metadata property) 
    {
        // Check if this factory can handle the property
        return property.PropertyType == typeof(MyCustomType);
    }
    
    public ControlContext? CreateControl(
        Metadata property, 
        object model, 
        Action<object?> onValueChanged) 
    {
        // Create and return custom control context
        var control = new MyCustomControl();
        return new MyCustomControlContext(control, onValueChanged);
    }
}
```

### Handler Pipeline

The watch handler pipeline enables plug-and-play reactive behaviors:

1. **Auto-Discovery**: Handlers register via `AddFormServices()` (scans assembly for `IWatchFactory`)
2. **Property Matching**: Each factory checks `CanHandle()` during binding
3. **Handler Creation**: Matching factories create handler instances via `OnCreateInstance()`
4. **Initialization**: `OnInitialize()` sets up dependencies and metadata
5. **Reactive Execution**: `OnRefresh()` executes when watched properties change

**Template Method Pattern**:
```csharp
public abstract class WatchHandler : IWatchHandler
{
    protected virtual void OnInitialize(object model) { }  // Setup
    protected virtual void OnRefresh(object model, bool isFirst) { }  // React to changes
    protected override void OnDispose() { }  // Cleanup
}
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Links

- [GitHub Repository](https://github.com/jlopez788/CoreOne.Winforms)
- [NuGet Package](https://www.nuget.org/packages/CoreOne.Winforms)
- [Report Issues](https://github.com/jlopez788/CoreOne.Winforms/issues)

## 📝 Requirements

- .NET 9.0 or later
- Windows OS (WinForms)
- Visual Studio 2022 or later (recommended)

---

**Made with ❤️ by Juan Lopez**
