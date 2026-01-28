using System.Diagnostics;

namespace CoreOne.Winforms.Models;

/// <summary>
/// Represents a dropdown item with display text and actual value
/// </summary>
[DebuggerDisplay("{Display}")]
public class DropdownItem(string display, object? value)
{
    /// <summary>
    /// Gets or sets the display text shown in the dropdown
    /// </summary>
    public string Display { get; init; } = display;

    /// <summary>
    /// Gets or sets the actual value assigned to the property
    /// </summary>
    public object? Value { get; set; } = value;

    public DropdownItem() : this(string.Empty, null)
    {
    }

    public DropdownItem(string value) : this(value, value)
    {
    }
}