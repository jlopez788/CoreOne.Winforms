using CoreOne.Reactive;
using CoreOne.Winforms.Events;

namespace CoreOne.Winforms;

/// <summary>
/// Manages two-way data binding between model properties and WinForms controls.
/// Provides transaction-like behavior with commit/rollback support for change management.
/// </summary>
/// <remarks>
/// The IModelBinder orchestrates the form generation process:
/// <list type="number">
/// <item><description>Discovers properties via reflection</description></item>
/// <item><description>Selects appropriate control factories based on priority</description></item>
/// <item><description>Creates controls with two-way binding</description></item>
/// <item><description>Registers watch handlers for reactive behaviors</description></item>
/// <item><description>Tracks changes and provides undo/redo capabilities</description></item>
/// </list>
/// </remarks>
public interface IModelBinder
{
    /// <summary>
    /// Gets an observable stream of property change notifications.
    /// </summary>
    /// <value>A reactive subject that emits <see cref="ModelPropertyChanged"/> events whenever a bound property value changes.</value>
   // Subject<ModelPropertyChanged> PropertyChanged { get; }

    /// <summary>
    /// Binds a model's properties to controls within the specified container, automatically generating
    /// labels, controls, and layout based on property attributes.
    /// </summary>
    /// <param name="context">The model context containing the model instance and metadata for binding.</param>
    /// <returns>The ideal size required to display all generated controls in the layout.</returns>
    Panel BindModel(ModelContext context);

    ///// <summary>
    ///// Commits all pending changes to the bound model, making them permanent.
    ///// </summary>
    //void Commit();

    ///// <summary>
    ///// Rolls back all pending changes to the bound model, restoring property values
    ///// to their state at the last <see cref="Commit"/> or initial binding.
    ///// </summary>
    //void Rollback();

    ///// <summary>
    ///// Unbinds the current model, clearing all control associations and releasing resources.
    ///// </summary>
    //void UnbindModel();
}