using CoreOne.Reactive;
using CoreOne.Winforms.Events;

namespace CoreOne.Winforms;

/// <summary>
/// Interface for binding model properties to controls
/// </summary>
public interface IModelBinder
{
    Subject<ModelPropertyChanged> PropertyChanged { get; }

    /// <summary>
    /// Binds a model to a collection of controls
    /// </summary>
    /// <returns>The ideal size for all rendered controls</returns>
    Size BindModel(Control container, object model);

    /// <summary>
    /// Gets the currently bound model
    /// </summary>
    object? GetBoundModel();

    /// <summary>
    /// Unbinds the current model
    /// </summary>
    void UnbindModel();
}