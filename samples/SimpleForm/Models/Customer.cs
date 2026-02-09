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
    [Required]
    [GridColumn(GridColumnSpan.Half)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [GridColumn(GridColumnSpan.Half)]
    public string LastName { get; set; } = string.Empty;
    // Computed property for display name
    [Compute(nameof(GetFullName))]
    [WatchProperties(nameof(FirstName), nameof(LastName))]
    [GridColumn(GridColumnSpan.Full)]
    public string FullName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [GridColumn(GridColumnSpan.Half)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [ReadOnly(true)]
    [GridColumn(GridColumnSpan.Half)]
    public string? Phone { get; set; }

    [Required]
    [DropdownSource(typeof(CountryProvider))]
    [GridColumn(GridColumnSpan.Two)]
    public string Country { get; set; } = string.Empty;

    // State dropdown depends on Country selection
    [DropdownSource(typeof(StateProvider))]
    [WatchProperties(nameof(Country))]
    [GridColumn(GridColumnSpan.Two)]
    public string? State { get; set; }

    [DropdownSource(typeof(IndustryProvider))]
    [GridColumn(GridColumnSpan.Two)]
    public string? Industry { get; set; }
    [Required]
    [Adult]
    [GridColumn(GridColumnSpan.Full)]
    public DateTime DateOfBirth { get; set; } = DateTime.Now.AddYears(-19);

    [GridColumn(GridColumnSpan.Half)]
    public bool IsActive { get; set; } = true;

    [File(multiselect: false)]
    [EnabledWhen(nameof(IsActive), true)]
    public string? File { get; set; }

    // This field is only enabled when IsActive is true
    [EnabledWhen(nameof(IsActive), true)]
    [GridColumn(GridColumnSpan.Full)]
    public string? ActiveNotes { get; set; }

    // Rating control (1-5 stars)
    [Rating(5)]
    [GridColumn(GridColumnSpan.Half)]
    public int CustomerRating { get; set; } = 3;

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