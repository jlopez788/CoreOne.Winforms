using CoreOne.Winforms;
using CoreOne.Winforms.Models;

namespace SimpleFormExample.Providers;

/// <summary>
/// Provides a list of countries for dropdown selection
/// </summary>
public class CountryProvider : IDropdownSourceProviderSync
{
    public void Initialize(IWatchHandler handler)
    {
        // No initialization needed for static list
    }

    public IEnumerable<DropdownItem> GetItems(object model)
    {
        return new[]
        {
            new DropdownItem { Display = "United States", Value = "US" },
            new DropdownItem { Display = "Canada", Value = "CA" },
            new DropdownItem { Display = "United Kingdom", Value = "UK" },
            new DropdownItem { Display = "Germany", Value = "DE" },
            new DropdownItem { Display = "France", Value = "FR" },
            new DropdownItem { Display = "Japan", Value = "JP" },
            new DropdownItem { Display = "Australia", Value = "AU" }
        };
    }

    public void Dispose()
    {
        // No resources to dispose
    }
}
