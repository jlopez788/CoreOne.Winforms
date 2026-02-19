namespace CoreOne.Winforms.Extensions;

public static class MetadataExtensions
{
    public static void OnAttribute<TAttribute>(this Metadata provider, Action<TAttribute> callback) where TAttribute : Attribute
    {
        var attribute = provider.GetCustomAttribute<TAttribute>();
        if (attribute != null)
        {
            callback(attribute);
        }
    }

    public static void OnAttributes<TAttribute>(this Metadata provider, Action<IEnumerable<TAttribute>> callback) where TAttribute : Attribute
    {
        var attributes = provider.GetCustomAttributes<TAttribute>();
        if (attributes != null && attributes.Any())
        {
            callback(attributes);
        }
    }
}
