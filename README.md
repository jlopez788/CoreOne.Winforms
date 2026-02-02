# CoreOne.Winforms

A modern, feature-rich WinForms library that provides **dynamic model-based form generation** with automatic two-way data binding, themed controls, and smooth animations. Built for .NET 9.0 with dependency injection support.

[![NuGet](https://img.shields.io/nuget/v/CoreOne.Winforms.svg)](https://www.nuget.org/packages/CoreOne.Winforms)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
## 🚀 Features

### Dynamic Form Generation
- **ModelControl** - Automatically generates form fields from your model properties
- **Two-way data binding** - Changes in controls update the model, and vice versa
- **Reactive updates** - Built on reactive patterns with `Subject<T>` for change notifications
- **Dirty tracking** - Know when data has been modified

### Smart Grid Layout
- **6-column Bootstrap-style grid**
- **`[GridColumn]` attribute** - Control field width (1-6 columns)
- **Auto-sizing** - Calculates ideal form dimensions
- **Labels above controls** - Clean, modern layout

### Dropdown Support
- **Type-based dropdown sources** - `[DropdownSource<T>]` attribute
- **Dependency tracking** - `[DropdownDependsOn]` for cascading dropdowns
- **Sync & Async providers** - Support for both data retrieval patterns
- **Auto-refresh** - Dependent dropdowns update automatically

### Themed Controls
- **OButton** - Modern button with hover/press states
- **AnimatedPanel** - Smooth transitions and animations
- **LoadingCircle** - Loading indicator control
- **BaseView** - Base control for creating custom views

### Advanced Features
- **Rating Control** - Star rating control with `[Rating]` attribute for numeric properties
- **Conditional Enabled** - `[EnabledWhen]` attribute for dynamic control state
- **Property Watching** - `[WatchProperty]` for reactive property monitoring
- **Validation support** - Built-in validation with `ValidateModel` extension
- **Undo/Redo** - `RejectChanges()` and `AcceptChanges()` for change management
- **Custom attributes** - `[Ignore]`, `[Visible]`, and more
- **Dependency injection** - First-class DI support
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
    
    [DropdownSource<CountryProvider>]
    [GridColumn(GridColumnSpan.Half)]
    public string Country { get; set; }
    
    [DropdownSource<StateProvider>]
    [WatchProperty(nameof(Country))]  // Refreshes when Country changes
    [GridColumn(GridColumnSpan.Half)]
    public string State { get; set; }
    
    public DateTime BirthDate { get; set; }
    
    public bool IsActive { get; set; }
    
    [EnabledWhen(nameof(IsActive), true)]  // Only enabled when IsActive is true
    public string ActiveNotes { get; set; }
    
    [Rating(5)]  // Display as 5-star rating control
    public int CustomerRating { get; set; }
    
    [Ignore]  // Won't generate a control
    public int InternalId { get; set; }
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
    private IDropdownContext? _context;
    
    public void Initialize(IDropdownContext context)
    {
        _context = context;
        // Subscribe to changes, setup refresh logic, etc.
    }
    
    public IEnumerable<DropdownItem> GetItems(object model)
    {
        // Fetch from database, API, etc.
        return new[]
        {
            new DropdownItem("United States", "US"),
            new DropdownItem("Canada", "CA"),
            new DropdownItem("Mexico", "MX")
        };
    }
    
    public void Dispose()
    {
        // Cleanup resources
    }
}

public class StateProvider : IDropdownSourceProviderSync
{
    public void Initialize(IDropdownContext context) { }
    
    public IEnumerable<DropdownItem> GetItems(object model)
    {
        // Access the model to get dependent property value
        var customer = (Customer)model;
        
        return customer.Country switch
        {
            "US" => new[] 
            { 
                new DropdownItem("California", "CA"),
                new DropdownItem("Texas", "TX"),
                new DropdownItem("New York", "NY")
            },
            "CA" => new[] 
            { 
                new DropdownItem("Ontario", "ON"),
                new DropdownItem("British Columbia", "BC")
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
    
    public void Initialize(DropdownContext context) { }
    
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

### Monitoring Property Changes

```csharp
var token = SToken.Create();

_modelControl.PropertyChanged?.Subscribe(change =>
{
    Console.WriteLine($"Property '{change.PropertyName}' changed from " +
    WatchProperty("PropertyName")]` | Watch property for changes | `[WatchProperty(nameof(Country))]` |
| `[EnabledWhen("PropertyName", value)]` | Conditionally enable control | `[EnabledWhen(nameof(IsActive), true)]` |
| `[Rating(maxRating)]` | Display as star rating control | `[Rating(5
}, token);

// Later, dispose the token to unsubscribe
token.Dispose();
```

### Available Attributes

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[GridColumn(GridColumnSpan)]` | Control column span (1-6) | `[GridColumn(GridColumnSpan.Full)]` |
| `[DropdownSource<T>]` | Specify dropdown provider | `[DropdownSource<CountryProvider>]` |
| `[DropdownDependsOn("PropertyName")]` | Declare dropdown dependency | `[DropdownDependsOn(nameof(Country))]` |
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
   

### RatingControl - Star Rating

```csharp
var rating = new RatingControl
{
    MaxRating = 5,
    Value = 3,RefreshManager`
- **Factory Pattern** - Composite factory with priority-based control creation
- **Dependency Injection** - Constructor injection throughout
- **Reactive Programming** - Observable property changes with `Subject<T>`
- **Property Watching** - Reactive property monitoring with automatic UI updates
- **Extensible** - Add custom control factories with priority support

### Factory Priority System

Control factories use a priority system to determine which factory handles a property:
- **Priority 100+**: Attribute-based factories (`[Rating]`, `[DropdownSource]`)
- **Priority 0**: Generic type-based factories (String, Numeric, Boolean, DateTime, Enum)

Custom factories can specify their priority:
```csharp
public class MyCustomFactory : IPropertyControlFactory
{
    public int Priority => 50; // Medium priority
    // ...
}
```

rating.ValueChanged += (s, e) => 
{
    Console.WriteLine($"Rating changed to: {rating.Value}");
};
``` Color = Color.Blue
};
```

## 🏗️ Architecture

CoreOne.Winforms follows **SOLID principles** with a clean, testable architecture:

- **Services** - `IModelBinder`, `IPropertyControlFactory`, `IDropdownRefreshManager`
- **Factory Pattern** - Composite factory for control creation
- **Dependency Injection** - Constructor injection throughout
- **Reactive Programming** - Observable property changes
- **Repository Pattern** - Ready for your data layer

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
