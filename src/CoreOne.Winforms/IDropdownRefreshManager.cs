using CoreOne.Winforms.Services;

namespace CoreOne.Winforms;

/// <summary>
/// Interface for managing dropdown refresh logic based on property dependencies
/// </summary>
public interface IDropdownRefreshManager
{
    /// <summary>
    /// Registers a dropdown control that needs to be refreshed when dependencies change
    /// </summary>
    void RegisterDropdown(DropdownContext context, object model);

    /// <summary>
    /// Notifies the manager that a property value has changed
    /// </summary>
    void NotifyPropertyChanged(object model, string propertyName, object? newValue);

    /// <summary>
    /// Unregisters all dropdowns
    /// </summary>
    void Clear();
}