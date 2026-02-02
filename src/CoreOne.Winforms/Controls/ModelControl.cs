using CoreOne.Reactive;
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
    public OButton BtnSave { get; private set; } = default!;
    public bool IsDirty { get; private set; }
    public Subject<ModelPropertyChanged>? PropertyChanged => ModelBinder?.PropertyChanged;

    public ModelControl()
    {
    }

    public ModelControl(IServiceProvider services, IModelBinder modelBinder)
    {
        Services = services;
        ModelBinder = modelBinder;

        InitializeComponent();
    }

    public void AcceptChanges()
    {
        IsDirty = false;
        ModelBinder?.Commit();
    }

    /// <summary>
    /// Gets the currently bound model
    /// </summary>
    public T? GetModel<T>() where T : class => ModelBinder?.GetBoundModel() as T;

    public void RejectChanges()
    {
        IsDirty = false;
        ModelBinder?.Rollback();
    }

    /// <summary>
    /// Sets the model and generates controls for its properties
    /// </summary>
    public void SetModel<T>(T model) where T : class
    {
        this.ClearControls();
        var idealSize = ModelBinder?.BindModel(this, model);
        var token = SToken.Create();

        IsDirty = false;
        ModelBinder?.PropertyChanged.Subscribe(_ => IsDirty = true, token);
        // Add Save button at the bottom
        if (idealSize.HasValue)
        {
            AddSaveButton(idealSize.Value);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        MoveButtons();
    }

    protected virtual void OnSaveClicked()
    {
        var model = ModelBinder?.GetBoundModel();
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
            Height = 35,
            Anchor = AnchorStyles.Top,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font(Font.FontFamily, 10, FontStyle.Regular)
        };

        // Center the button horizontally
        BtnSave.Location = new Point((Width / 2) - (BtnSave.Width / 2), contentSize.Height + 26);
        BtnSave.Click += (s, e) => OnSaveClicked();

        Controls.Add(BtnSave);
        BtnSave.BringToFront();
        MoveButtons();
    }

    private void MoveButtons()
    {
        if (BtnSave is null)
            return;

        var buttonLeft = (Width / 2) - (BtnSave.Width / 2);
        BtnSave.Location = new Point(buttonLeft, BtnSave.Location.Y);
    }
}