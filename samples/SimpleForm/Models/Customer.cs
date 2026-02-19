using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;
using SimpleFormExample.Attributes;
using SimpleFormExample.Providers;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SimpleFormExample.Models;

/// <summary>
/// Customer model demonstrating CoreOne.Winforms attribute-driven form generation
/// </summary>
public class Customer
{
    [Group(1)]
    [GridColumn(GridColumnSpan.Half)]
    public string SSN { get; set; } = string.Empty;

    [Group(1)]
    [GridColumn(GridColumnSpan.Half)]
    public string DriverLicense { get; set; } = string.Empty;

    [Group(2)]
    [Required]
    [GridColumn(GridColumnSpan.Half)]
    public string FirstName { get; set; } = string.Empty;

    [Group(2)]
    [Required]
    [GridColumn(GridColumnSpan.Half)]
    public string LastName { get; set; } = string.Empty;

    [Group(2)]
    [Compute(nameof(GetFullName))]
    [WatchProperties(nameof(FirstName), nameof(LastName))]
    [GridColumn(GridColumnSpan.Full)]
    public string FullName { get; set; } = string.Empty;

    [Group(2)]
    [Required]
    [Adult]
    [GridColumn(GridColumnSpan.Full)]
    public DateTime DateOfBirth { get; set; } = DateTime.Now.AddYears(-19);

    [Group(3)]
    [Required]
    [EmailAddress]
    [GridColumn(GridColumnSpan.Half)]
    public string Email { get; set; } = string.Empty;

    [Group(3)]
    [Phone]
    [ReadOnly(true)]
    [GridColumn(GridColumnSpan.Half)]
    public string? Phone { get; set; }

    [Group(4)]
    [Required]
    [DropdownSource(typeof(CountryProvider))]
    [GridColumn(GridColumnSpan.Two)]
    public string Country { get; set; } = string.Empty;

    [Group(4)]
    // State dropdown depends on Country selection
    [DropdownSource(typeof(StateProvider))]
    [WatchProperties(nameof(Country))]
    [GridColumn(GridColumnSpan.Two)]
    public string? State { get; set; }

    [Group(5)]
    [DropdownSource(typeof(IndustryProvider))]
    //[GridColumn(GridColumnSpan.Full)]
    public string? Industry { get; set; }

    [Group(6)]
    [GridColumn(GridColumnSpan.Half)]
    public bool IsActive { get; set; } = true;

    [Group(6)]
    [File(multiselect: false)]
    [EnabledWhen(nameof(IsActive), true)]
    public string? File { get; set; }

    [Group(6)]
    // This field is only enabled when IsActive is true
    [EnabledWhen(nameof(IsActive), true)]
    [GridColumn(GridColumnSpan.Full)]
    public string? ActiveNotes { get; set; }

    [Group(7)]
    // Rating control (1-5 stars)
    [Rating(5)]
    [GridColumn(GridColumnSpan.Half)]
    public int CustomerRating { get; set; } = 3;

    [Group(7)]
    // Computed property that calculates based on other properties
    [Compute(nameof(CalculateTotalScore))]
    [WatchProperties(nameof(CustomerRating), nameof(IsActive))]
    [GridColumn(GridColumnSpan.Half)]
    public decimal TotalScore { get; set; }

    // This property won't generate a control (internal use only)
    [Ignore]
    public int InternalId { get; set; }

    

    public decimal CalculateTotalScore(int customerRating) => IsActive ? customerRating * 10 : 0;

    public string GetFullName(string firstName) => $"{firstName} {LastName}".Trim();
}