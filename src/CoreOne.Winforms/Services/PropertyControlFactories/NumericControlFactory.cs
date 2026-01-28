using System.ComponentModel.DataAnnotations;

namespace CoreOne.Winforms.Services.PropertyControlFactories;

public class NumericControlFactory : IPropertyControlFactory
{
    public bool CanHandle(Metadata property)
    {
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return Types.IsNumberType(propertyType) ||
               underlyingType == typeof(int) || underlyingType == typeof(long) ||
               underlyingType == typeof(short) || underlyingType == typeof(byte) ||
               underlyingType == typeof(decimal) || underlyingType == typeof(double) ||
               underlyingType == typeof(float);
    }

    public (Control? control, Action<object?>? setValue) CreateControl(Metadata property, object model, Action<object?> onValueChanged)
    {
        var propertyType = property.FPType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        var numericUpDown = new NumericUpDown();

        // Configure based on type
        var type = Types.Void;
        var byteType = typeof(byte);
        if (underlyingType == byteType)
        {
            type = byteType;
            numericUpDown.Minimum = 0;
            numericUpDown.Maximum = 255;
        }
        else if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
        {
            type = underlyingType;
            numericUpDown.DecimalPlaces = 2;
            numericUpDown.Minimum = decimal.MinValue;
            numericUpDown.Maximum = decimal.MaxValue;
        }
        else
        {
            type = Types.Int;
            numericUpDown.Minimum = int.MinValue;
            numericUpDown.Maximum = int.MaxValue;
        }

        var range = property.GetCustomAttribute<RangeAttribute>();
        if (range is not null)
        {
            numericUpDown.Minimum = Convert.ToDecimal(range.Minimum);
            numericUpDown.Maximum = Convert.ToDecimal(range.Maximum);
            numericUpDown.Increment = 1;
        }

        numericUpDown.ValueChanged += (s, e) => {
            object? convertedValue = underlyingType switch {
                Type t when t == typeof(int) => (int)numericUpDown.Value,
                Type t when t == typeof(long) => (long)numericUpDown.Value,
                Type t when t == typeof(short) => (short)numericUpDown.Value,
                Type t when t == typeof(byte) => (byte)numericUpDown.Value,
                Type t when t == typeof(decimal) => numericUpDown.Value,
                Type t when t == typeof(double) => (double)numericUpDown.Value,
                Type t when t == typeof(float) => (float)numericUpDown.Value,
                _ => null
            };
            onValueChanged(convertedValue);
        };

        return (numericUpDown, UpdateControlValue);

        void UpdateControlValue(object? value)
        {
            if (value != null)
            {
                try
                {
                    numericUpDown.Value = Convert.ToDecimal(value);
                }
                catch
                {
                    // Ignore conversion errors
                }
            }
        }
    }
}