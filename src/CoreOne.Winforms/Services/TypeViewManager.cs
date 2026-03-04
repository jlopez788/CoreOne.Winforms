using CoreOne.Collections;
using CoreOne.Reactive;
using CoreOne.Winforms.Controls;
using CoreOne.Winforms.Events;
using System.Reflection;

namespace CoreOne.Winforms.Services;

public class TypeViewManager : Data<string, Type>
{
    protected Func<Type, IView> ResolveCallback { get; }

    public TypeViewManager(Func<Type, IView>? resolve = null) : base(StringComparer.OrdinalIgnoreCase)
    {
        ResolveCallback = resolve ?? new Func<Type, IView>(type => (IView)type.GetInstance());
    }

    public TypeViewManager(IServiceProvider services) : base(StringComparer.OrdinalIgnoreCase)
    {
        var targetCreator = new TargetCreator(services);
        ResolveCallback = type => (IView)(services.GetService(type) ?? targetCreator.CreateInstance(type)!);
    }

    public void RegisterViews(Assembly assembly)
    {
        var view = typeof(BaseView);
        assembly.GetTypes()
            .Where(t => !t.IsAbstract && TypeUtility.IsSubclassOfRawGeneric(view, t))
            .Each(t => Set(t.Name.Replace("View", ""), t));
    }

    public IView? Resolve(ViewEventArgs eventArgs) => OnResolve(eventArgs);

    protected virtual IView? CreateInstance(Type type) => ResolveCallback(type);

    protected virtual IView? OnResolve(ViewEventArgs eventArgs)
    {
        if (eventArgs.Name is not null && TryGetValue(eventArgs.Name, out var type))
            return CreateInstance(type);

        var key = eventArgs.Name?.Replace("View", "");
        return key is not null && TryGetValue(key, out type) ? CreateInstance(type) : null;
    }
}