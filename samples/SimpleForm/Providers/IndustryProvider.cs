using CoreOne.Winforms;
using CoreOne.Winforms.Models;

namespace SimpleFormExample.Providers;

/// <summary>
/// Provides a list of industries for customer classification
/// </summary>
public class IndustryProvider : IDropdownSourceProviderSync
{
    public void Initialize(IWatchHandler handler)
    {
        // No initialization needed
    }

    public IEnumerable<DropdownItem> GetItems(object model)
    {
        return
        [
            new DropdownItem { Display = "Technology", Value = "TECH" },
            new DropdownItem { Display = "Healthcare", Value = "HEALTH" },
            new DropdownItem { Display = "Finance", Value = "FIN" },
            new DropdownItem { Display = "Retail", Value = "RETAIL" },
            new DropdownItem { Display = "Manufacturing", Value = "MFG" },
            new DropdownItem { Display = "Education", Value = "EDU" },
            new DropdownItem { Display = "Government", Value = "GOV" },
            new DropdownItem { Display = "Other", Value = "OTHER" }
        ];
    }

    public void Dispose()
    {
        // No resources to dispose
    }
}