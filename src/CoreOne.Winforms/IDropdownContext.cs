using CoreOne.Winforms.Models;

namespace CoreOne.Winforms;

/// <summary>
/// Context provided to dropdown source providers for rebinding and refreshing
/// </summary>
public interface IDropdownContext
{
    /// <summary>
    /// Requests a refresh of the dropdown with new items
    /// </summary>
    /// <param name="items">The new items to display</param>
    void RefreshItems(IEnumerable<DropdownItem> items);

    /// <summary>
    /// Requests a refresh of the dropdown by re-evaluating the source
    /// </summary>
    void RequestRefresh(object model);
}