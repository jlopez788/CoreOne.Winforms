using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CoreOne.Winforms.Services;

/// <summary>
/// Creates property grid items with vertical layout (label above control)
/// </summary>
public class PropertyGridItemFactory : IPropertyGridItemFactory
{
    private readonly IPropertyControlFactory ControlFactory;
    private readonly Func<Metadata, string?>[] DisplayAttributes;

    public PropertyGridItemFactory(IPropertyControlFactory controlFactory)
    {
        ControlFactory = controlFactory ?? throw new ArgumentNullException(nameof(controlFactory));
        DisplayAttributes = [
            m=>getName<DisplayAttribute>(m,p=>p.Name),
            m=>getName<DisplayNameAttribute>(m,p=>p.DisplayName),
            m=>getName<DescriptionAttribute>(m,p=>p.Description),
            m=>string.Concat(m.Name.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()))
        ];

        static string? getName<T>(Metadata property, Func<T, string?> callback) where T : Attribute
        {
            var attribute = property.GetCustomAttribute<T>();
            return attribute is not null ? callback(attribute) : null;
        }
    }

    public PropertyGridItem? CreatePropertyGridItem(Metadata property, object model, Action<object?> onValueChanged)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(onValueChanged);

        if (!ControlFactory.CanHandle(property))
            return null;

        // Create label
        var label = new Label {
            Text = Format(property),
            AutoSize = true,
            Dock = DockStyle.Top,
            Height = 20
        };

        // Create control - use special method for dropdowns that support refresh
        var (inputControl, setvalue) = ControlFactory.CreateControl(property, model, onValueChanged);
        if (inputControl == null)
            return null;

        inputControl.Dock = DockStyle.Top;
        inputControl.Height = 25;

        // Create container panel with vertical layout
        var container = new Panel {
            Dock = DockStyle.Fill,
            AutoSize = true,
            MinimumSize = new Size(0, 45)
        };

        var visible = property.GetCustomAttribute<VisibleAttribute>()?.IsVisible;
        container.Controls.Add(inputControl);
        container.Controls.Add(label); // Add label last so it appears on top with Dock.Top

        var columnSpan = GetColumnSpan(property);

        // Set initial value
        var currentValue = property.GetValue(model);
        setvalue?.Invoke(currentValue);

        return new PropertyGridItem {
            Property = property,
            Label = label,
            InputControl = inputControl,
            Container = container,
            ColumnSpan = columnSpan
        };

        GridColumnSpan GetColumnSpan(Metadata property)
        {
            var attribute = property.GetCustomAttribute<GridColumnAttribute>();
            return visible.GetValueOrDefault(true) ?
                    attribute?.Span ?? GridColumnSpan.Default :
                    GridColumnSpan.None;
        }

        string Format(Metadata property)
        {
            return DisplayAttributes.Select(p => p(property))
                 .FirstOrDefault(p => !string.IsNullOrEmpty(p)) ?? string.Empty;
        }
    }
}