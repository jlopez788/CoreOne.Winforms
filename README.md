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
- **Conditional Enabled** - `[EnabledWhen]` attribute for dynamic control state
- **Property Watching** - `[WatchProperties]` for reactive property monitoring
- **Validation support** - Built-in validation with `ValidateModel` extension
- **Undo/Redo** - `RejectChanges()` and `AcceptChanges()` for change management
- **Custom attributes** - `[Ignore]`, `[Visible]`, `[Rating]`, and more
- **Single instance apps** - Built-in support for preventing multiple instances
- **Dependency injection** - First-class DI support throughout
- **SOLID architecture** - Clean, maintainable codebase with factory priority system

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
    [WatchProperties(nameof(Country))]  // Refreshes when Country changes
    [GridColumn(GridColumnSpan.Half)]
    public string State { get; set; }
    
    public DateTime BirthDate { get; set; }
    
    public bool IsActive { get; set; }
    
    [EnabledWhen(nameof(IsActive), true)]  // Only enabled when IsActive is true
    public string ActiveNotes { get; set; }
    
    [Rating(5)]  // Display as 5-star rating control
    public int CustomerRating { get; set; }
    
    [Compute("CalculateTotalScore")]
    [WatchProperties(nameof(CustomerRating), nameof(IsActive))]
    public decimal TotalScore { get; set; }
    
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

```csharp
using CoreOne.Winforms;
using CoreOne.Winforms.Models;

public class CountryProvider : IDropdownSourceProviderSync
{
    public void Initialize(IWatchHandler context)
    {
        // Subscribe to changes, setup refresh logic, etc.
    }
    
    public IEnumerable<DropdownItem> GetItems(object model)
    {
        // Fetch from database, API, etc.
        return new[]
        {
            new DropdownItem("US", "United States"),
            new DropdownItem("CA", "Canada"),
            new DropdownItem("MX", "Mexico")
        };
    }
    
    public void Dispose()
    {
        // Cleanup resources
    }
}

public class StateProvider : IDropdownSourceProviderSync
{
    public void Initialize(IWatchHandler context) { }
    
    public IEnumerable<DropdownItem> GetItems(object model)
    {
        // Access the model to get dependent property value
        var customer = (Customer)model;
        
        return customer.Country switch
        {
            "US" => new[] 
            { 
                new DropdownItem("CA", "California"),
                new DropdownItem("TX", "Texas"),
                new DropdownItem("NY", "New York")
            },
            "CA" => new[] 
            { 
                new DropdownItem("ON", "Ontario"),
                new DropdownItem("BC", "British Columbia")
            },
            _ => Array.Empty<DropdownItem>()
        };
    }
    
    public void Dispose() { }
}
```

### Async Dropdown Provider

```csharp
public class ProductCategoriesProvider : IDropdownSourceProviderAsync
{
    private readonly HttpClient _httpClient;
    
    public ProductCategoriesProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public void Initialize(IWatchHandler context) { }
    
    public async Task<IEnumerable<DropdownItem>> GetItemsAsync(object model)
    {
        var response = await _httpClient.GetFromJsonAsync<Category[]>("/api/categories");
        return response?.Select(c => new DropdownItem(c.Id.ToString(), c.Name)) 
               ?? Array.Empty<DropdownItem>();
    }
    
    public void Dispose() 
    {
        _httpClient?.Dispose();
    }
}
```

### Creating Custom Watch Handlers

The library uses an extensible handler pipeline for reactive behaviors. You can create custom handlers:

```csharp
using CoreOne.Winforms;
using CoreOne.Winforms.Services.WatchHandlers;

// 1. Create a custom attribute
[AttributeUsage(AttributeTargets.Property)]
public class VisibleWhenAttribute : WatchPropertiesAttribute
{
    public object ExpectedValue { get; }
    public string PropertyName { get; }
    
    public VisibleWhenAttribute(string propertyName, object expectedValue) 
        : base(propertyName)
    {
        PropertyName = propertyName;
        ExpectedValue = expectedValue;
    }
}

// 2. Create the handler
public class VisibleWhenHandler : WatchFactoryFromAttribute<VisibleWhenAttribute>, IWatchFactory
{
    private class VisibleWhenHandlerInstance : WatchHandler
    {
        private readonly PropertyGridItem _gridItem;
        private readonly VisibleWhenAttribute _attribute;
        private Metadata? _targetProperty;
        
        public VisibleWhenHandlerInstance(PropertyGridItem gridItem, VisibleWhenAttribute attr)
            : base(gridItem.Property)
        {
            _gridItem = gridItem;
            _attribute = attr;
        }
        
        protected override void OnInitialize(object model)
        {
            _targetProperty = MetaType.GetMetadata(model.GetType(), _attribute.PropertyName);
        }
        
        protected override void OnRefresh(object model)
        {
            if (_targetProperty == null) return;
            
            var value = _targetProperty.GetValue(model);
            var isVisible = Equals(value, _attribute.ExpectedValue);
            
            _gridItem.InputControl?.CrossThread(() => 
                _gridItem.InputControl.Visible = isVisible);
        }
    }
    
    protected override IWatchHandler? OnCreateInstance(
        PropertyGridItem gridItem, VisibleWhenAttribute[] attributes)
    {
        return new VisibleWhenHandlerInstance(gridItem, attributes[0]);
    }
}

// 3. Register it (happens automatically via AddFormServices if in same assembly)
// Or manually: services.AddSingleton<IWatchFactory, VisibleWhenHandler>();

// 4. Use it
public class MyModel
{
    public bool ShowAdvanced { get; set; }
    
    [VisibleWhen(nameof(ShowAdvanced), true)]
    public string AdvancedSetting { get; set; }
}
```

### Monitoring Property Changes

```csharp
var token = SToken.Create();

_modelControl.PropertyChanged?.Subscribe(change =>
{
    Console.WriteLine($"Property '{change.PropertyName}' changed from " +
                     $"'{change.OldValue}' to '{change.NewValue}'");
}, token);

// Later, dispose the token to unsubscribe
token.Dispose();
```

### Available Attributes

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[GridColumn(GridColumnSpan)]` | Control column span (1-6) | `[GridColumn(GridColumnSpan.Full)]` |
| `[DropdownSource(typeof(Provider))]` | Specify dropdown provider | `[DropdownSource(typeof(CountryProvider))]` |
| `[WatchProperties("prop1", "prop2")]` | Declare property dependencies | `[WatchProperties(nameof(Country))]` |
| `[EnabledWhen("PropertyName", value)]` | Conditionally enable control | `[EnabledWhen(nameof(IsActive), true)]` |
| `[Compute("MethodName")]` | Execute method to compute value | `[Compute("CalculateTotal")]` |
| `[Rating(maxRating)]` | Display as star rating control | `[Rating(5)]` |
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

CoreOne.Winforms follows **SOLID principles** with a clean, testable architecture:

### Core Services
- **IModelBinder** - Manages two-way binding between model and controls
- **IPropertyGridItemFactory** - Creates property grid items with labels
- **IGridLayoutManager** - Handles 6-column Bootstrap-style grid layout
- **IRefreshManager** - Coordinates property change notifications across handlers
- **FormContainerHostService** - Manages hosted form services lifecycle

### Extensibility
- **IControlFactory** - Factory pattern for creating controls based on property types
  - Priority-based system (higher priority = checked first)
  - Attribute-based factories (Priority 100+): `[Rating]`, `[DropdownSource]`
  - Type-based factories (Priority 0): String, Numeric, Boolean, DateTime, Enum
- **IWatchFactory** - Factory pattern for creating reactive property handlers
  - `EnabledWhenHandler` - Conditional control enabling
  - `ComputeHandler` - Computed property values
  - `DropdownHandler` - Dropdown refresh on dependency changes
- **Dependency Injection** - Constructor injection throughout
- **Reactive Programming** - Observable property changes with `Subject<T>`

### Factory Priority System

Control and watch factories use a priority system to determine which factory handles a property:
- **Priority 100+**: Attribute-based factories (`[Rating]`, `[DropdownSource]`, `[Compute]`)
- **Priority 0**: Generic type-based factories (String, Numeric, Boolean, DateTime, Enum)

Custom factories can specify their priority:
```csharp
public class MyCustomFactory : IControlFactory
{
    public int Priority => 50; // Medium priority
    public bool CanHandle(Metadata property) { /* ... */ }
    public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged) { /* ... */ }
}
```

### Handler Pipeline

The watch handler pipeline enables plug-and-play reactive behaviors:

1. **Discovery**: Handlers are auto-registered via `AddFormServices()`
2. **Matching**: During binding, handlers check if they can handle each property
3. **Initialization**: Handler instances are created and initialized
4. **Execution**: When dependencies change, handlers execute in priority order

See [WATCH_CONTEXT_ARCHITECTURE.md](WATCH_CONTEXT_ARCHITECTURE.md) for detailed handler architecture documentation.

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
