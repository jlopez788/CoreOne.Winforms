using CoreOne.Reactive;
using CoreOne.Winforms.Events;
using Microsoft.Extensions.DependencyInjection;

namespace CoreOne.Winforms.Services;

/// <summary>
/// Manages binding between model properties and controls using a 6-column grid layout
/// </summary>
public class ModelBinder(IServiceProvider services, IGridLayoutManager layoutManager) : Disposable, IModelBinder, IDisposable
{
    private readonly ErrorProvider ErrorProvider = new();
    private readonly List<IControlFactory> Factories = [.. services.GetRequiredService<IEnumerable<IControlFactory>>().OrderByDescending(p => p.Priority)];
    private readonly List<IWatchFactory> Handlers = [.. services.GetRequiredService<IEnumerable<IWatchFactory>>().OrderByDescending(p => p.Priority)];

    public Subject<ModelPropertyChanged> PropertyChanged { get; } = new();

    public Panel BindModel(ModelContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        ErrorProvider.Clear();

        var model = context.Model;
        var propertyGroups = context.GetGroupEntries();
        var itemFactory = services.GetRequiredService<IPropertyGridItemFactory>();
        var allGroups = new List<(Control control, GridColumnSpan columnSpan)>();
        foreach (var group in propertyGroups)
        {
            var groupItems = new List<PropertyGridItem>();
            foreach (var property in group.Properties)
            {
                var controlFactory = Factories.FirstOrDefault(p => p.CanHandle(property));
                if (controlFactory is null)
                    continue;

                var gridItem = itemFactory.CreatePropertyGridItem(controlFactory, property, model, value => onValueChanged(property, value), ErrorProvider);
                if (gridItem != null)
                {
                    Handlers
                        .Select(p => p.CreateInstance(gridItem))
                        .ExcludeNulls()
                        .Each(p => context.RegisterContext(p, model));

                    groupItems.Add(gridItem);
                }
            }

            var (control, height) = layoutManager.RenderLayout(groupItems);
            if (height > 0)
            {
                // control.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                if (group.GroupId != GroupDetail.GROUP_ID)
                {
                    var gc = context.GetGroup(group.GroupId);
                    var groupBox = new GroupBox {
                        Text = gc?.Title ?? "<>",
                        Size = new Size(control.Width + 20, control.Height + 52),
                        Dock = DockStyle.Fill
                    };
                    var span = context.GetGridColumnSpan(group.GroupId);
                    control.Dock = DockStyle.Fill;
                    groupBox.Controls.Add(control);
                    allGroups.Add((groupBox, span));
                }
                else
                {
                    allGroups.Add((control, context.GetGridColumnSpan(group.GroupId)));
                }
            }
        }

        var cells = layoutManager.CalculateLayout(allGroups);
        var (flow, outerHeight) = layoutManager.RenderLayout(cells);
        flow.ParentChanged += (s, e) => ErrorProvider.ContainerControl = flow.FindForm();
        //flow.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
        return flow;

        void onValueChanged(Metadata property, object? value)
        {
            var current = property.GetValue(context.Model);
            if (Equals(current, value))
                return;

            property.SetValue(context.Model, value);

            var args = new ModelPropertyChanged(property, context.Model, value);
            PropertyChanged.OnNext(args);

            // Notify dropdown refresh manager
            context.NotifyPropertyChanged(model, property.Name, value);
        }
    }

    protected override void OnDispose()
    {
        PropertyChanged.Dispose();
        ErrorProvider.Dispose();

        base.OnDispose();
    }
}