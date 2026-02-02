using CoreOne.Reactive;
using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Events;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CoreOne.Winforms.Services;

/// <summary>
/// Manages binding between model properties and controls using a 6-column grid layout
/// </summary>
public class ModelBinder(IServiceProvider services, IRefreshManager refreshManager, IGridLayoutManager layoutManager) : Disposable, IModelBinder, IDisposable
{
    private readonly IPropertyGridItemFactory ControlFactory = services.GetRequiredService<IPropertyGridItemFactory>();
    private readonly List<PropertyGridItem> GridItems = [];
    private ModelTransaction? Transaction;
    public Subject<ModelPropertyChanged> PropertyChanged { get; } = new();

    public Size BindModel(Control container, object model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(container);

        UnbindModel();

        Transaction = new ModelTransaction(model);

        var properties = MetaType.GetMetadatas(model.GetType(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
            .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<IgnoreAttribute>() is null)
            .ToList();

        foreach (var property in properties)
        {
            var gridItem = ControlFactory.CreatePropertyGridItem(property, model,
                value => {
                    var current = property.GetValue(Transaction.Model);
                    if (Equals(current, value))
                        return;

                    UpdateModelProperty(property, value);

                    var args = new ModelPropertyChanged(property, Transaction.Model, value);
                    PropertyChanged.OnNext(args);

                    // Notify dropdown refresh manager
                    refreshManager.NotifyPropertyChanged(model, property.Name, value);
                });

            if (gridItem != null)
            {
                var enabledWatches = property.GetCustomAttributes<EnabledWhenAttribute>();
                enabledWatches.Each(attr => {
                    var watch = new EnabledContext(gridItem.InputControl, property, attr);
                    refreshManager.RegisterContext(watch, Transaction.Model);
                });
                GridItems.Add(gridItem);
            }
        }

        // Calculate grid layout
        var itemsWithSpans = GridItems.Select(item => (item.Container as Control, item.ColumnSpan));
        var gridCells = layoutManager.CalculateLayout(itemsWithSpans);

        // Render layout
        var (layoutPanel, height) = layoutManager.RenderLayout(gridCells);

        container.Controls.Add(layoutPanel);

        // Calculate ideal size
        return new Size(container.Width, height);
    }

    public void Commit() => Transaction?.Commit();

    public object? GetBoundModel() => Transaction?.Model;

    public void Rollback() => Transaction?.Rollback();

    public void UnbindModel()
    {
        GridItems.Clear();
        Transaction?.Dispose();
        refreshManager.Clear();
    }

    protected override void OnDispose()
    {
        PropertyChanged.Dispose();
        base.OnDispose();
    }

    private void UpdateModelProperty(Metadata property, object? value)
    {
        if (Transaction?.Model is null)
            return;

        property.SetValue(Transaction.Model, value);
    }
}