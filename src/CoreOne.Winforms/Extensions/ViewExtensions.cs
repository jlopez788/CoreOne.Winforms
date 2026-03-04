using CoreOne.Winforms.Events;
using System.Diagnostics.CodeAnalysis;

namespace CoreOne.Winforms.Extensions;

public static class ViewExtensions
{
    public static bool GetIndex<T>(this ViewEventArgs eventArgs, int idx, Action<T> callback)
    {
        var args = eventArgs.Args;
        if (args?.Count > 0 && idx < args.Count && args[idx] is T t)
        {
            callback?.Invoke(t);
            return callback is not null;
        }

        return false;
    }

    public static bool TryGetIndex<T>(this ViewEventArgs eventArgs, int idx, [NotNullWhen(true)] out T? model)
    {
        model = default;
        var args = eventArgs.Args;
        if (args?.Count > 0 && idx < args.Count && args[idx] is T m)
        {
            model = m;
            return model is not null;
        }
        return false;
    }
}