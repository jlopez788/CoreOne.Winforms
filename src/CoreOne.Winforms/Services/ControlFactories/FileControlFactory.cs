using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Services.ControlFactories;

public class FileControlFactory : IControlFactory
{
    private class FileControlContext(Control control, Action<object?> onValueChanged, Action<object?> setValue) : ControlContext(control, setValue)
    {
        public Metadata Property { get; init; } = default!;
        public Button Button { get; init; } = default!;
        public TextBox TextBox { get; init; } = default!;
        private OpenFileDialog? FileDialog;

        protected override void OnBindEvent()
        {
            var attribute = Property.GetCustomAttribute<FileAttribute>();
            if (attribute != null)
            {
                FileDialog = new OpenFileDialog {
                    Filter = attribute.Filter,
                    Multiselect = attribute.Multiselect
                };
            }

            Button.Click += OnButtonClick;
        }

        protected override void OnUnbindEvent()
        {
            Button.Click -= OnButtonClick;
            FileDialog?.Dispose();
            FileDialog = null;
        }

        private void OnButtonClick(object? sender, EventArgs e)
        {
            if (FileDialog?.ShowDialog() == DialogResult.OK)
            {
                var result = FileDialog.Multiselect
                    ? string.Join(";", FileDialog.FileNames)
                    : FileDialog.FileName;

                UpdateValue(result);
                onValueChanged?.Invoke(result);
            }
        }
    }

    public int Priority => 50;

    public bool CanHandle(Metadata property) => property.GetCustomAttribute<FileAttribute>() != null;

    public ControlContext? CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var panelWidth = 80;
        var panel = new Panel {
            Location = new Point(0, 0),
            Size = new Size(panelWidth, 24)
        };
        var textBox = new TextBox {
            Location = new Point(0, 0),
            Size = new Size(panelWidth - 32, 24),
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Enabled = false,
            ReadOnly=true
        };
        var button = new Button {
            Text = "...",
            Location = new Point(textBox.Width, 0),
            Size = new Size(panelWidth - textBox.Width, 24),
            Anchor = AnchorStyles.Right
        };
        button.BringToFront();

        panel.Controls.Add(textBox);
        panel.Controls.Add(button);

        return new FileControlContext(panel, onValueChanged,
            value => textBox.Text = value?.ToString()) {
            Property = property,
            Button = button,
            TextBox = textBox,
        };
    }
}