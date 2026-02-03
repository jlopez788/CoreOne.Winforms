namespace CoreOne.Winforms;

/// <summary>
/// Interface for providing dropdown items
/// </summary>
public interface IDropdownSourceProvider : IDisposable
{
    /// <summary>
    /// Initializes the provider with context for rebinding and refreshing
    /// </summary>
    /// <param name="handler">Handler</param>
    void Initialize(IWatchHandler handler);
}

/// <summary>
/// Interface for async dropdown sources that accept parameters based on other property values
/// </summary>
public interface IDropdownSourceProviderSync : IDropdownSourceProvider
{
    /// <summary>
    /// Gets the dropdown items asynchronously based on parameter values
    /// </summary>
    IEnumerable<DropdownItem> GetItems(object model);
}

/// <summary>
/// Interface for dropdown sources that accept parameters based on other property values
/// </summary>
public interface IDropdownSourceProviderAsync : IDropdownSourceProvider
{
    /// <summary>
    /// Gets the dropdown items based on parameter values
    /// </summary>
    Task<IEnumerable<DropdownItem>> GetItemsAsync(object model);
}