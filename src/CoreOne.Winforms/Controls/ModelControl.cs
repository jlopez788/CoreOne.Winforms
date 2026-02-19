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
    private Size IdealSize = new(1, 1);
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

        AddSaveButton(PnlControls.Size);

        //PnlView.BackColor = Color.DarkOrchid;
        PnlView.MouseEnter += (s, e) => PnlView.Invalidate();
        PnlView.MouseLeave += (s, e) => PnlView.Invalidate();
        PnlView.Paint += (s, e) => {
            using var pen = new SolidBrush(Color.Red);
            e.Graphics.FillRectangle(pen, PnlView.Width - 10, 0, 10, IdealSize.Height);
        };
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
        PnlView.ClearControls();
        IdealSize = ModelBinder?.BindModel(PnlView, model) ?? new Size(1, 1);
        var token = SToken.Create();

        IsDirty = false;
        ModelBinder?.PropertyChanged.Subscribe(_ => IsDirty = true, token);
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
        BtnSave.Location = new Point((contentSize.Width / 2) - (BtnSave.Width / 2), contentSize.Height / 2 - BtnSave.Height / 2);
        BtnSave.Click += (s, e) => OnSaveClicked();

        PnlControls.Controls.Add(BtnSave);
        BtnSave.BringToFront();
    }
}