namespace CoreOne.Winforms.Services.WatchHandlers;

public abstract class WatchFactoryFromAttribute<TAttribute> : IWatchFactory where TAttribute : Attribute
{
    public IWatchHandler? CreateInstance(PropertyGridItem gridItem)
    {
        var attributes = gridItem.Property.GetCustomAttributes<TAttribute>().ToArray();
        return CanHandle(gridItem, attributes) ? OnCreateInstance(gridItem, attributes) : null;
    }

    protected virtual bool CanHandle(PropertyGridItem gridItem, TAttribute[] attributes) => attributes.Length >= 1;

    protected abstract IWatchHandler? OnCreateInstance(PropertyGridItem gridItem, TAttribute[] attributes);
}