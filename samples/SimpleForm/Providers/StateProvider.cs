using CoreOne.Winforms;
using CoreOne.Winforms.Models;

namespace SimpleFormExample.Providers;

/// <summary>
/// Provides a list of states/provinces based on the selected country
/// Demonstrates cascading dropdown functionality
/// </summary>
public class StateProvider : IDropdownSourceProviderSync
{
    public void Initialize(IWatchHandler handler)
    {
        // Initialization happens once during binding
    }

    public IEnumerable<DropdownItem> GetItems(object model)
    {
        // Get the selected country from the model
        var country = model?.GetType()
            .GetProperty("Country")
            ?.GetValue(model)?.ToString();

        return country switch
        {
            "US" => GetUSStates(),
            "CA" => GetCanadianProvinces(),
            "UK" => GetUKRegions(),
            "DE" => GetGermanStates(),
            "AU" => GetAustralianStates(),
            _ => Enumerable.Empty<DropdownItem>()
        };
    }

    public void Dispose()
    {
        // No resources to dispose
    }

    private IEnumerable<DropdownItem> GetUSStates()
    {
        return new[]
        {
            new DropdownItem { Display = "California", Value = "CA" },
            new DropdownItem { Display = "Texas", Value = "TX" },
            new DropdownItem { Display = "Florida", Value = "FL" },
            new DropdownItem { Display = "New York", Value = "NY" },
            new DropdownItem { Display = "Illinois", Value = "IL" }
        };
    }

    private IEnumerable<DropdownItem> GetCanadianProvinces()
    {
        return new[]
        {
            new DropdownItem { Display = "Ontario", Value = "ON" },
            new DropdownItem { Display = "Quebec", Value = "QC" },
            new DropdownItem { Display = "British Columbia", Value = "BC" },
            new DropdownItem { Display = "Alberta", Value = "AB" }
        };
    }

    private IEnumerable<DropdownItem> GetUKRegions()
    {
        return new[]
        {
            new DropdownItem { Display = "England", Value = "ENG" },
            new DropdownItem { Display = "Scotland", Value = "SCT" },
            new DropdownItem { Display = "Wales", Value = "WLS" },
            new DropdownItem { Display = "Northern Ireland", Value = "NIR" }
        };
    }

    private IEnumerable<DropdownItem> GetGermanStates()
    {
        return new[]
        {
            new DropdownItem { Display = "Bavaria", Value = "BY" },
            new DropdownItem { Display = "Berlin", Value = "BE" },
            new DropdownItem { Display = "Hamburg", Value = "HH" }
        };
    }

    private IEnumerable<DropdownItem> GetAustralianStates()
    {
        return new[]
        {
            new DropdownItem { Display = "New South Wales", Value = "NSW" },
            new DropdownItem { Display = "Victoria", Value = "VIC" },
            new DropdownItem { Display = "Queensland", Value = "QLD" }
        };
    }
}
