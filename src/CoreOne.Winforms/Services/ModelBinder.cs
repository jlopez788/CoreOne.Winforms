using CoreOne.Collections;
using CoreOne.Reactive;
using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Events;
using CoreOne.Winforms.Models;
using System.Reflection;

namespace CoreOne.Winforms.Services;

/// <summary>
/// Manages binding between model properties and controls using a 6-column grid layout
/// </summary>
public class ModelBinder(
    IPropertyGridItemFactory gridItemFactory,
    IGridLayoutManager layoutManager,
    IDropdownRefreshManager refreshManager) : Disposable, IModelBinder, IDisposable
{
    private readonly IPropertyGridItemFactory _gridItemFactory = gridItemFactory ?? throw new ArgumentNullException(nameof(gridItemFactory));
    private readonly Data<Metadata, PropertyGridItem> _propertyItems = [];
    private object? _model;
    public Subject<ModelPropertyChanged> PropertyChanged { get; } = new();

    public Size BindModel(Control container, object model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(container);

        UnbindModel();
        _model = model;

        var properties = MetaType.GetMetadatas(model.GetType(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
            .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<IgnoreAttribute>() is null)
            .ToList();

        // Create property grid items
        var gridItems = new List<PropertyGridItem>();
        foreach (var property in properties)
        {
            var gridItem = _gridItemFactory.CreatePropertyGridItem(property, model,
                value => {
                    UpdateModelProperty(property, value);

                    var args = new ModelPropertyChanged(property, _model, value);
                    PropertyChanged.OnNext(args);

                    // Notify dropdown refresh manager
                    refreshManager.NotifyPropertyChanged(model, property.Name, value);
                });

            if (gridItem != null)
            {
                gridItems.Add(gridItem);
                _propertyItems[property] = gridItem;
            }
        }

        // Calculate grid layout
        var itemsWithSpans = gridItems.Select(item => (item.Container as Control, item.ColumnSpan));
        var gridCells = layoutManager.CalculateLayout(itemsWithSpans);

        // Render layout
        var (layoutPanel, height) = layoutManager.RenderLayout(gridCells);

        container.Controls.Add(layoutPanel);

        // Calculate ideal size
        return new Size(container.Width, height);
    }

    public object? GetBoundModel() => _model;

    public void UnbindModel()
    {
        _propertyItems.Clear();
        refreshManager.Clear();
        _model = null;
    }

    protected override void OnDispose()
    {
        PropertyChanged.Dispose();
        base.OnDispose();
    }

    private void UpdateModelProperty(Metadata property, object? value)
    {
        if (_model == null)
            return;

        property.SetValue(_model, value);
    }
}