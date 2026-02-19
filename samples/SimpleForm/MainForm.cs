using CoreOne;
using CoreOne.Extensions;
using CoreOne.Reactive;
using CoreOne.Winforms;
using CoreOne.Winforms.Controls;
using CoreOne.Winforms.Events;
using Microsoft.Extensions.DependencyInjection;
using SimpleFormExample.Models;

namespace SimpleFormExample;

/// <summary>
/// Main form demonstrating CoreOne.Winforms ModelControl usage
/// </summary>
public partial class MainForm : Form
{
    private readonly IModelBinder _modelBinder;
    private readonly IServiceProvider _services;
    private Customer _customer = null!;
    private ModelControl? _modelControl;
    private SToken Token;

    public MainForm(IServiceProvider services)
    {
        Token = this.CreateSToken();
        _services = services;
        _modelBinder = services.GetRequiredService<IModelBinder>();

        InitializeComponent();
        LoadCustomer();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_modelControl != null && _modelControl.IsDirty)
        {
            var result = MessageBox.Show(
                this,
                "You have unsaved changes. Do you want to exit anyway?",
                "Unsaved Changes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        base.OnFormClosing(e);
    }

    private void InitializeComponent()
    {
        Text = "CoreOne.Winforms - Simple Form Example";
        Size = new Size(800, 700);
        BackColor = Color.White;
        StartPosition = FormStartPosition.CenterScreen;

        // Add menu strip
        var menuStrip = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("File");
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("New", null, (s, e) => LoadCustomer()));
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("Reset", null, (s, e) => ResetForm()));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("Exit", null, (s, e) => Close()));
        menuStrip.Items.Add(fileMenu);

        var helpMenu = new ToolStripMenuItem("Help");
        helpMenu.DropDownItems.Add(new ToolStripMenuItem("About", null, (s, e) => ShowAbout()));
        menuStrip.Items.Add(helpMenu);

        Controls.Add(menuStrip);

        // Add title label
        var titleLabel = new Label {
            Text = "Customer Information",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 40),
            ForeColor = Color.FromArgb(0, 120, 215)
        };
        Controls.Add(titleLabel);

        // Add instruction label
        var instructionLabel = new Label {
            Text = "Fill out the customer information below. Watch how the State dropdown updates when you change Country, " +
                   "and how the Total Score is computed automatically based on Rating and Active status.",
            AutoSize = false,
            Size = new Size(760, 50),
            Location = new Point(20, 75),
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray
        };
        Controls.Add(instructionLabel);
    }

    private void LoadCustomer()
    {
        // Create a new customer with sample data
        _customer = new Customer {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "555-1234",
            Country = "US",
            State = "CA",
            Industry = "TECH",
            IsActive = true,
            ActiveNotes = "Preferred customer since 2020",
            CustomerRating = 4,
            InternalId = 12345
        };

        // Remove existing ModelControl if present
        if (_modelControl != null)
        {
            Controls.Remove(_modelControl);
            _modelControl.Dispose();
        }

        // Create new ModelControl
        _modelControl = new ModelControl(_services, _modelBinder) {
            Location = new Point(20, 135),
            Width = 760,
            Height = 460,
            AutoScroll = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        // Subscribe to property changes (reactive pattern)
        //var builder = new StringBuilder();
        //var bounce = new Debounce(() => {
        //    MessageBox.Show(builder.ToString(), "Updates", MessageBoxButtons.OK);
        //    builder.Clear();
        //}, 1500);
        _modelControl.PropertyChanged?.Subscribe(change => {
            Console.WriteLine($"Property changed: {change.Property.Name} = {change.NewValue}");

            //builder.AppendLine($"Property changed: {change.Property.Name} = {change.NewValue}");
            //bounce.Invoke();
        }, Token);

        _modelControl.SaveClicked += OnSaveClicked;
        _modelControl.SetModel(_customer);

        Controls.Add(_modelControl);

        // Adjust form height to fit all controls
        Height = _modelControl.Bottom + 100;
    }

    private void OnSaveClicked(object? sender, ModelSavedEventArgs e)
    {
        if (!e.Validation.IsValid)
        {
            var errors = string.Join("\n", e.Validation.ErrorMessages ?? Enumerable.Empty<string>());
            MessageBox.Show(
                this,
                $"Please fix the following errors:\n\n{errors}",
                "Validation Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var customer = e.Target as Customer;
        if (customer == null)
            return;

        var message = $"Customer saved successfully!\n\n" +
                     $"Name: {customer.FullName}\n" +
                     $"Email: {customer.Email}\n" +
                     $"Country: {customer.Country}\n" +
                     $"State: {customer.State ?? "N/A"}\n" +
                     $"Industry: {customer.Industry ?? "N/A"}\n" +
                     $"Rating: {customer.CustomerRating} stars\n" +
                     $"Total Score: {customer.TotalScore}\n" +
                     $"Active: {customer.IsActive}\n" +
                     $"File: {customer.File}\n" +
                     $"\nModified: {(e.IsModified ? "Yes" : "No")}";

        MessageBox.Show(
            this,
            message,
            "Save Successful",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        // Mark as not dirty after successful save
        if (_modelControl != null)
        {
            _modelControl.AcceptChanges();
        }
    }

    private void ResetForm()
    {
        if (_modelControl != null && _modelControl.IsDirty)
        {
            var result = MessageBox.Show(
                this,
                "Are you sure you want to reset all changes?",
                "Confirm Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _modelControl.RejectChanges();
            }
        }
    }

    private void ShowAbout()
    {
        MessageBox.Show(
            this,
            "CoreOne.Winforms Simple Form Example\n\n" +
            "This example demonstrates:\n" +
            "• Automatic form generation from model properties\n" +
            "• Attribute-driven configuration\n" +
            "• Two-way data binding\n" +
            "• Cascading dropdowns (Country → State)\n" +
            "• Computed properties (Total Score)\n" +
            "• Conditional enabling (Active Notes)\n" +
            "• Rating controls\n" +
            "• Validation support\n" +
            "• Dirty tracking and undo/redo",
            "About",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}