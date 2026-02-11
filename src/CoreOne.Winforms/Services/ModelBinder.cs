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
    private readonly ErrorProvider ErrorProvider = new();
    private readonly List<IControlFactory> Factories = [.. services.GetRequiredService<IEnumerable<IControlFactory>>()];
    private readonly List<PropertyGridItem> GridItems = [];
    private readonly List<IWatchFactory> Handlers = [.. services.GetRequiredService<IEnumerable<IWatchFactory>>()];
    private ModelTransaction? Transaction;

    public Subject<ModelPropertyChanged> PropertyChanged { get; } = new();

    public Size BindModel(Control container, object model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(container);

        UnbindModel();

        Transaction = new ModelTransaction(model);

        var itemFactory = services.GetRequiredService<IPropertyGridItemFactory>();
        var factories = Factories.OrderByDescending(f => f.Priority).ToList();
        var properties = MetaType.GetMetadatas(model.GetType(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
            .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<IgnoreAttribute>() is null)
            .ToList();
        var handlers = Handlers.OrderByDescending(p => p.Priority).ToList();
        ErrorProvider.Clear();
        foreach (var property in properties)
        {
            var controlFactory = factories.FirstOrDefault(p => p.CanHandle(property));
            if (controlFactory is null)
                continue;

            var gridItem = itemFactory.CreatePropertyGridItem(controlFactory, property, model, value => {
                var current = property.GetValue(Transaction.Model);
                if (Equals(current, value))
                    return;

                UpdateModelProperty(property, value);

                var args = new ModelPropertyChanged(property, Transaction.Model, value);
                PropertyChanged.OnNext(args);

                // Notify dropdown refresh manager
                refreshManager.NotifyPropertyChanged(model, property.Name, value);
            }, ErrorProvider);

            if (gridItem != null)
            {
                handlers
                    .OrderByDescending(p => p.Priority)
                    .Select(p => p.CreateInstance(gridItem))
                    .ExcludeNulls()
                    .Each(p => refreshManager.RegisterContext(p, model));

                GridItems.Add(gridItem);
            }
        }

        // Calculate grid layout
        var itemsWithSpans = GridItems.Select(item => ((Control)item.Container, item.ColumnSpan));
        var gridCells = layoutManager.CalculateLayout(itemsWithSpans);

        // Render layout
        var (layoutPanel, height) = layoutManager.RenderLayout(gridCells);

        container.Controls.Add(layoutPanel);

        // Set ErrorProvider's ContainerControl to enable icon display
        ErrorProvider.ContainerControl = container as ContainerControl ?? container.FindForm();

        // Calculate ideal size
        return new Size(container.Width, height);
    }

    public void Commit() => Transaction?.Commit();

    public object? GetBoundModel() => Transaction?.Model;

    public void Rollback()
    {
        if (Transaction is null)
            return;

        Transaction.Rollback();
        foreach (var item in GridItems)
        {
            item.ControlContext.UnbindEvent();

            var value = item.Property.GetValue(Transaction.Model);
            refreshManager.NotifyPropertyChanged(Transaction.Model, item.Property.Name, value);
            item.ControlContext.UpdateValue(value);
            item.ControlContext.BindEvent();
        }
    }

    public void UnbindModel()
    {
        GridItems.Each(p => p.Dispose());
        GridItems.Clear();
        refreshManager.Clear();
        Transaction?.Dispose();
        Transaction = null;
    }

    protected override void OnDispose()
    {
        PropertyChanged.Dispose();
        ErrorProvider.Dispose();
        base.OnDispose();
    }

    private void UpdateModelProperty(Metadata property, object? value)
    {
        if (Transaction?.Model is null)
            return;

        property.SetValue(Transaction.Model, value);
    }
}