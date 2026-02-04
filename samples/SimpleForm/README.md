# CoreOne.Winforms - Simple Form Example

This example demonstrates the core features of CoreOne.Winforms library including automatic form generation, two-way data binding, cascading dropdowns, computed properties, and validation.

## Features Demonstrated

### 1. **Automatic Form Generation**
- Model properties automatically generate form controls
- Labels are created from property names with formatting
- No manual control creation required

### 2. **Attribute-Driven Configuration**
- `[GridColumn]` - Controls field width in 6-column grid
- `[DropdownSource]` - Creates dropdown controls from providers
- `[Rating]` - Creates star rating controls
- `[EnabledWhen]` - Conditional control enabling
- `[Compute]` - Calculated/computed properties
- `[WatchProperties]` - Monitors property changes
- `[Ignore]` - Excludes properties from form generation

### 3. **Cascading Dropdowns**
- **Country** dropdown with static list of countries
- **State** dropdown that updates automatically when Country changes
- Demonstrates reactive data binding with `[WatchProperties]`

### 4. **Computed Properties**
- **TotalScore** - Automatically calculated from CustomerRating × 10 (if active)
- **FullName** - Automatically combines FirstName and LastName
- Updates in real-time as dependencies change

### 5. **Conditional Enabling**
- **ActiveNotes** field - Only enabled when IsActive checkbox is checked
- Demonstrates form logic without code-behind

### 6. **Data Validation**
- Required fields (FirstName, LastName, Email, Country)
- Email format validation
- Phone format validation
- Validation runs on save with user feedback

### 7. **Dirty Tracking & Undo**
- Form tracks unsaved changes (`IsDirty` flag)
- **Save** button commits changes
- **Reset** menu item undoes changes (rollback)
- Warns before closing with unsaved changes

## Running the Example

1. **Build the Solution**
   ```bash
   dotnet build CoreOne.Winforms.sln
   ```

2. **Run the Example**
   ```bash
   cd samples\SimpleForm
   dotnet run
   ```

## Project Structure

```
SimpleForm/
├── Program.cs              # Application entry point with DI setup
├── MainForm.cs             # Main form demonstrating ModelControl
├── Models/
│   └── Customer.cs         # Customer model with attributes
└── Providers/
    ├── CountryProvider.cs  # Country dropdown provider
    ├── StateProvider.cs    # State dropdown provider (cascading)
    └── IndustryProvider.cs # Industry dropdown provider
```

## Code Walkthrough

### 1. Model Definition (Customer.cs)

```csharp
public class Customer
{
    [Required]
    [GridColumn(GridColumnSpan.Half)]  // 50% width
    public string FirstName { get; set; }

    [DropdownSource(typeof(CountryProvider))]
    [GridColumn(GridColumnSpan.Half)]
    public string Country { get; set; }

    // State refreshes when Country changes
    [DropdownSource(typeof(StateProvider))]
    [WatchProperties(nameof(Country))]
    [GridColumn(GridColumnSpan.Half)]
    public string? State { get; set; }

    // Only enabled when IsActive is true
    [EnabledWhen(nameof(IsActive), true)]
    public string? ActiveNotes { get; set; }

    [Rating(5)]  // 5-star rating
    public int CustomerRating { get; set; }

    // Computed from CustomerRating and IsActive
    [Compute(nameof(CalculateTotalScore))]
    [WatchProperties(nameof(CustomerRating), nameof(IsActive))]
    public decimal TotalScore { get; set; }

    public decimal CalculateTotalScore(int customerRating, bool isActive)
    {
        return isActive ? customerRating * 10 : 0;
    }
}
```

### 2. Dependency Injection Setup (Program.cs)

```csharp
var services = new ServiceCollection();

// Register CoreOne.Winforms services
services.AddFormServices();

// Register dropdown providers
services.AddSingleton<CountryProvider>();
services.AddSingleton<StateProvider>();

var serviceProvider = services.BuildServiceProvider();
```

### 3. Creating the Form (MainForm.cs)

```csharp
// Create ModelControl
_modelControl = new ModelControl(_services, _modelBinder);

// Subscribe to property changes
_modelControl.PropertyChanged?.Subscribe(change =>
{
    Console.WriteLine($"Property changed: {change.PropertyName}");
});

// Subscribe to save event
_modelControl.SaveClicked += OnSaveClicked;

// Bind the model (generates controls automatically)
_modelControl.SetModel(_customer);
```

### 4. Handling Save

```csharp
private void OnSaveClicked(object? sender, ModelSavedEventArgs e)
{
    if (!e.Validation.IsValid)
    {
        // Show validation errors
        MessageBox.Show(errors, "Validation Failed");
        return;
    }

    // Model is valid - save to database, etc.
    _modelControl.AcceptChanges();
}
```

### 5. Cascading Dropdown Provider (StateProvider.cs)

```csharp
public class StateProvider : IDropdownSourceProviderSync
{
    public IEnumerable<DropdownItem> GetItems(object model)
    {
        // Get the selected country from the model
        var country = model.GetType()
            .GetProperty("Country")
            ?.GetValue(model)?.ToString();

        return country switch
        {
            "US" => GetUSStates(),
            "CA" => GetCanadianProvinces(),
            _ => Enumerable.Empty<DropdownItem>()
        };
    }
}
```

## Grid Layout System

CoreOne.Winforms uses a 6-column Bootstrap-style grid:

| Span  | Columns | Width | Use Case              |
|-------|---------|-------|-----------------------|
| One   | 1       | 16.67%| Very narrow fields    |
| Two   | 2       | 33.33%| Narrow fields         |
| Half  | 3       | 50%   | Side-by-side fields   |
| Four  | 4       | 66.67%| Wide fields           |
| Five  | 5       | 83.33%| Very wide fields      |
| Full  | 6       | 100%  | Full-width fields     |

## Attribute Reference

### Layout Attributes
- **[GridColumn(GridColumnSpan.Half)]** - Controls width (1-6 columns)
- **[Ignore]** - Excludes property from form generation

### Control Creation Attributes
- **[DropdownSource(typeof(Provider))]** - Creates dropdown using provider
- **[Rating(maxStars)]** - Creates star rating control

### Reactive Behavior Attributes
- **[WatchProperties("Prop1", "Prop2")]** - Monitors properties for changes
- **[EnabledWhen("Property", value)]** - Conditional enabling
- **[Compute("MethodName")]** - Calculated properties
- **[Visible("Property", value)]** - Conditional visibility

### Validation Attributes
- **[Required]** - Field must have a value
- **[EmailAddress]** - Must be valid email format
- **[Phone]** - Must be valid phone number
- **[StringLength(max)]** - String length validation
- **[Range(min, max)]** - Numeric range validation

## Try These Modifications

1. **Add a New Field**
   ```csharp
   [GridColumn(GridColumnSpan.Full)]
   public string Address { get; set; }
   ```

2. **Make a Computed Property**
   ```csharp
   [Compute(nameof(GetAgeFromBirthDate))]
   [WatchProperties(nameof(DateOfBirth))]
   public int Age { get; set; }

   public int GetAgeFromBirthDate(DateTime dateOfBirth)
   {
       return DateTime.Now.Year - dateOfBirth.Year;
   }
   ```

3. **Add Conditional Visibility**
   ```csharp
   [Visible(nameof(Country), "US")]
   public string SSN { get; set; }
   ```

4. **Create a Custom Dropdown Provider**
   ```csharp
   public class CustomProvider : IDropdownSourceProviderSync
   {
       public void Initialize(IWatchHandler handler) { }
       
       public IEnumerable<DropdownItem> GetItems(object model)
       {
           // Your custom logic
           return items;
       }
   }
   ```

## Next Steps

- Review the [main README](../../README.md) for complete documentation
- Explore other attributes and features
- Create custom control factories for special input types
- Implement custom watch handlers for complex reactive behaviors
- Add async dropdown providers for database-driven dropdowns

## Questions or Issues?

- GitHub Issues: https://github.com/jlopez788/CoreOne.Winforms/issues
- Documentation: See README.md in the root directory
