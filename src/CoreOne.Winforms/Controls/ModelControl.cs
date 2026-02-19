using CoreOne.Winforms.Events;

namespace CoreOne.Winforms.Controls;

/// <summary>
/// Control that dynamically generates form fields for model properties
/// </summary>
public partial class ModelControl : UserControl
{
    /// <summary>
    /// Event raised when the Save button is clicked
    /// </summary>
    public event EventHandler<ModelSavedEventArgs>? SaveClicked;
    private readonly IModelBinder? ModelBinder;
    private readonly IServiceProvider Services = default!;
    private ModelContext? Current;
    private Size IdealSize = new(1, 1);
    public OButton BtnSave { get; private set; } = default!;
    public bool IsDirty => Current?.IsModified ?? false;

    public ModelControl()
    {
    }

    public ModelControl(IServiceProvider services, IModelBinder modelBinder)
    {
        Services = services;
        ModelBinder = modelBinder;

        InitializeComponent();

        AddSaveButton(PnlControls.Size);
    }

    public void AcceptChanges()
    {
        Current?.Commit();
    }

    /// <summary>
    /// Gets the currently bound model
    /// </summary>
    public T? GetModel<T>() where T : class => Current?.Model as T;

    public void RejectChanges()
    {
        Current?.Rollback();
    }

    /// <summary>
    /// Sets the model and generates controls for its properties
    /// </summary>
    public void SetModel(ModelContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Store the context
        Current = context;

        // Clear only the data container, not PnlControls
        PnlView.ClearControls();

        // Bind the model and get the container
        var container = ModelBinder?.BindModel(context);
        if (container is null)
            return;

        container.Dock = DockStyle.Fill;
        PnlView.Controls.Add(container);
        container.BringToFront();

        // Ensure PnlControls is on top
        PnlControls.BringToFront();
    }

    protected virtual void OnSaveClicked()
    {
        var model = Current?.Model;
        if (model is not null)
        {
            var validation = model.ValidateModel(Services, true);
            SaveClicked?.Invoke(this, new ModelSavedEventArgs(model) {
                IsModified = IsDirty,
                Validation = validation
            });
        }
    }

    private void AddSaveButton(Size contentSize)
    {
        BtnSave = new OButton {
            Text = "Save",
            Width = 120,
            Height = 30,
            Anchor = AnchorStyles.Bottom,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font(Font.FontFamily, 10, FontStyle.Regular)
        };

        // Center the button horizontally
        BtnSave.Location = new Point((contentSize.Width / 2) - (BtnSave.Width / 2), contentSize.Height / 2 - BtnSave.Height / 2);
        BtnSave.Click += (s, e) => OnSaveClicked();

        PnlControls.Controls.Add(BtnSave);
        BtnSave.BringToFront();

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Hello");
        BtnSave.ContextMenuStrip = contextMenu;
    }
}