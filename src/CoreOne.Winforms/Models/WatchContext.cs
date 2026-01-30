using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Models;

public class WatchContext
{
    private readonly Lock Sync = new();
    private volatile bool IsInitialized = false;
    public HashSet<string> Dependencies { get; } = [];
    public Metadata Property { get; }

    public WatchContext(Metadata property)
    {
        Property = property;
        Dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Dependencies.AddRange(property.GetCustomAttributes<WatchPropertyAttribute>()
            .Select(attr => attr.PropertyName));
    }

    public void RequestRefresh(object model)
    {
        using (Sync.EnterScope())
        {
            if (!IsInitialized)
            {
                OnInitialized(model);
                IsInitialized = true;
            }

            OnRefresh(model);
        }
    }

    protected virtual void OnInitialized(object model)
    { }

    protected virtual void OnRefresh(object model)
    { }
}