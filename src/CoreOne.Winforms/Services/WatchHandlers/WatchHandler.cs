using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Services.WatchHandlers;

public abstract class WatchHandler : IWatchHandler
{
    private readonly Lock Sync = new();
    private bool IsInitialized;
    public HashSet<string> Dependencies { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Metadata Property { get; }

    public WatchHandler(Metadata property)
    {
        Property = property;
        Dependencies.AddRange(Property.GetCustomAttributes<WatchPropertiesAttribute>()
            .SelectMany(attr => attr.PropertyNames));
    }

    public void Refresh(object model)
    {
        var isFirst = false;
        if (!IsInitialized)
        {
            using (Sync.EnterScope())
            {
                if (!IsInitialized)
                {
                    isFirst = true;
                    Utility.Try(() => OnInitialize(model));
                    IsInitialized = true;
                }
            }
        }
        OnRefresh(model, isFirst);
    }

    protected virtual void OnInitialize(object model)
    {
    }

    protected virtual void OnRefresh(object model, bool isFirst)
    { }
}