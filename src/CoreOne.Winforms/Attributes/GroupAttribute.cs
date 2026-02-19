namespace CoreOne.Winforms.Attributes;

/// <summary>
/// Groups properties together under a labeled GroupBox in the form layout
/// </summary>
/// <remarks>
/// Properties with the same group title will be visually grouped together in a GroupBox.
/// The layout system maintains the 6-column grid within each group.
/// </remarks>
/// <example>
/// <code>
/// public class Customer
/// {
///     [Group("Personal Information")]
///     public string FirstName { get; set; }
///
///     [Group("Personal Information")]
///     public string LastName { get; set; }
///
///     [Group("Contact Details")]
///     public string Email { get; set; }
/// }
/// </code>
/// </example>
/// <param name="groupId">Group ID</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class GroupAttribute(int groupId) : Attribute
{
    /// <summary>
    /// Gets the group title displayed on the GroupBox
    /// </summary>
    public int GroupId { get; } = groupId;
}