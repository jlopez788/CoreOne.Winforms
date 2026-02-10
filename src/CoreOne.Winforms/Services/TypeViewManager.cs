using CoreOne.Collections;
using CoreOne.Reactive;
using CoreOne.Winforms.Controls;
using System.Reflection;

namespace CoreOne.Winforms.Services;

public class TypeViewManager : Data<string, Type>
{
    protected Func<Type, IView> OnResolve { get; }

    public TypeViewManager(Func<Type, IView>? resolve = null) : base(StringComparer.OrdinalIgnoreCase)
    {
        var targetCreator = new TargetCreator(null);
        OnResolve = resolve ?? new Func<Type, IView>(type => (IView)targetCreator.CreateInstance(type)!);
    }

    public TypeViewManager(IServiceProvider services) : base(StringComparer.OrdinalIgnoreCase)
    {
        var targetCreator = new TargetCreator(services);
        OnResolve = type => (IView)(services.Resolve(type) ?? targetCreator.CreateInstance(type)!);
    }

    public void RegisterViews(Assembly assembly)
    {
        var view = typeof(BaseView);
        assembly.GetTypes()
            .Where(t => !t.IsAbstract && TypeUtility.IsSubclassOfRawGeneric(view, t))
            .Each(t => Set(t.Name.Replace("View", ""), t));
    }

    public IView? Resolve(string? name)
    {
        if (name is not null && TryGetValue(name, out var type))
            return OnResolve(type);

        var key = name?.Replace("View", "");
        return key is not null && TryGetValue(key, out type) ? OnResolve(type) : null;
    }
}