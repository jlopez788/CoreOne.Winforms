using System.ComponentModel.DataAnnotations;

namespace CoreOne.Winforms.Services.WatchHandlers;

public class ValidationHandler : WatchFactoryFromAttribute<ValidationAttribute>, IWatchFactory
{
    private class ValidationHandlerInstance(PropertyGridItem gridItem, ValidationAttribute[] attributes) : WatchHandler(gridItem.Property)
    {
        private Debounce<object> Debounce = default!;
        private Font Font = default!;

        protected override void OnInitialize(object model)
        {
            Debounce = new Debounce<object>(OnDebounce, 300);
            Font = gridItem.Label.Font;
            if (attributes.OfType<RequiredAttribute>().Any())
            {
                gridItem.Label.Text += " *";
            }
        }

        protected override void OnRefresh(object model, bool isFirst)
        {
            if (isFirst)
            { // Run immediately on first refresh for initial validation display
                ValidateAndSetError(model);
            }
            else
            {
                Debounce.Invoke(model);
            }
        }

        private void OnDebounce(object model) => gridItem.InputControl.CrossThread(() => ValidateAndSetError(model));

        private void ValidateAndSetError(object model)
        {
            var value = gridItem.Property.GetValue(model);
            var context = new ValidationContext(model);
            var errors = attributes.Select(p => p.GetValidationResult(value, context))
                .Select(p => p?.ErrorMessage)
                .ExcludeNullOrEmpty()
                .ToList();

            if (gridItem.ErrorProvider is not null)
            {
                var isValid = errors.Count == 0;
                var errorMessage = isValid ? string.Empty : string.Join(Environment.NewLine, errors);
                
                // Set icon alignment to appear to the right of the control
                gridItem.ErrorProvider.SetIconAlignment(gridItem.InputControl, ErrorIconAlignment.MiddleRight);
                gridItem.ErrorProvider.SetIconPadding(gridItem.InputControl, 2);
                gridItem.ErrorProvider.SetError(gridItem.InputControl, errorMessage);
                
                gridItem.Label.Font = isValid ? Font : new Font(Font.FontFamily, Font.Size, FontStyle.Bold);
                gridItem.Label.ForeColor = isValid ? SystemColors.ControlText : Color.Firebrick;
            }
        }
    }

    protected override IWatchHandler? OnCreateInstance(PropertyGridItem gridItem, ValidationAttribute[] attributes) => new ValidationHandlerInstance(gridItem, attributes);
}