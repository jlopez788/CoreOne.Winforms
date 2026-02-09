using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Services.WatchHandlers;

/// <summary>
/// Watch handler that conditionally enables or disables controls based on property values.
/// Supports complex conditional logic with comparison operators (EqualTo, NotEqualTo, GreaterThan, etc.).
/// </summary>
/// <remarks>
/// <para>Multiple [EnabledWhen] attributes on the same property are combined with AND logic.</para>
/// <para>The control is enabled only when ALL conditions are satisfied.</para>
/// </remarks>
/// <example>
/// <code>
/// // Enable only when IsActive is true
/// [EnabledWhen(nameof(IsActive), true)]
/// public string Notes { get; set; }
///
/// // Enable only when Age is greater than or equal to 18
/// [EnabledWhen(nameof(Age), 18, ComparisonType.GreaterThanOrEqualTo)]
/// public bool CanVote { get; set; }
///
/// // Multiple conditions (both must be true)
/// [EnabledWhen(nameof(IsActive), true)]
/// [EnabledWhen(nameof(Role), "Admin")]
/// public string AdminNotes { get; set; }
/// </code>
/// </example>
public class EnabledWhenHandler : WatchFactoryFromAttribute<EnabledWhenAttribute>, IWatchFactory
{
    private class EnabledWhenHandlerInstance(PropertyGridItem gridItem, EnabledWhenAttribute[] attributes) : WhenHandler(gridItem, attributes)
    {
        protected override void OnCondiition(object model, IReadOnlyList<bool> flags)
        {
            // Control is enabled only if ALL conditions are satisfied (AND logic)
            var shouldEnable = flags.All(f => f);
            PropertyGridItem.InputControl.Enabled = shouldEnable;
        }
    }

    protected override IWatchHandler? OnCreateInstance(PropertyGridItem gridItem, EnabledWhenAttribute[] attributes) => new EnabledWhenHandlerInstance(gridItem, attributes);
}