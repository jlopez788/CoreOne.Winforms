using CoreOne.Reactive;
using CoreOne.Reflection;
using CoreOne.Winforms.Transitions.ManagedType;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace CoreOne.Winforms.Transitions;

public class Transition(ITransitionType transitionMethod)
{
    internal class TransitionedPropertyInfo
    {
        public object FinalValue { get; set; } = default!;
        public object InitValue { get; set; } = default!;
        public IManagedType ManagedType { get; init; } = default!;
        public Metadata Metadata { get; init; } = default!;
        public object Target { get; init; } = default!;

        public TransitionedPropertyInfo Copy() => new() {
            InitValue = InitValue,
            FinalValue = FinalValue,
            Target = Target,
            Metadata = Metadata,
            ManagedType = ManagedType
        };
    }

    private class PropertyUpdateArgs(object t, Metadata meta, object v) : EventArgs
    {
        public Metadata Metadata { get; init; } = meta;
        public object Target { get; init; } = t;
        public object Value { get; init; } = v;
    }

    public event EventHandler? StepProperty;
    public event EventHandler? TransitionCompletedEvent;
    private static readonly Dictionary<Type, IManagedType> ManagedTypes = [];
    private readonly Lock Lock = new();
    private readonly ITransitionType? OTransition = transitionMethod;
    private readonly Subject<bool> Stream = new();
    private readonly Stopwatch Watch = new();
    public CancellationToken CancellationToken { get; set; }
    internal List<TransitionedPropertyInfo> TransitionedProperties { get; } = [];

    static Transition()
    {
        RegisterType(new ManagedType_Int());
        RegisterType(new ManagedType_Float());
        RegisterType(new ManagedType_Double());
        RegisterType(new ManagedType_Color());
        RegisterType(new ManagedType_String());
        RegisterType(new ManagedType_Point());
        RegisterType(new ManagedType_GradientColor());
    }

    public Transition Add<T>(T target, Expression<Func<T, object>> expression, object final) where T : notnull
    {
        MemberExpression? member = null;

        if (expression.Body is not MemberExpression)
            member = expression.Body is UnaryExpression unary ? unary.Operand as MemberExpression : null;

        if (member != null)
        {
            var meta = MetaType.CreateFromMemberInfo(typeof(T), member.Member);
            Add(target, meta, final);
        }

        return this;
    }

    public Transition Add(object target, string propertyname, object start, object final)
    {
        if (target != null)
        {
            var meta = MetaType.GetMetadata(target.GetType(), propertyname);
            meta.SetValue(target, start);
            Add(target, meta, final);
        }
        return this;
    }

    public Transition Add(object target, string propertyname, object final)
    {
        if (target != null)
        {
            var meta = MetaType.GetMetadata(target.GetType(), propertyname);
            Add(target, meta, final);
        }
        return this;
    }

    public Transition RegisterOnFrameCompleted(Action callback, SToken token)
    {
        Stream.Subscribe(p => callback.Invoke(), token);
        return this;
    }

    public void Run(CancellationToken cancellationToken = default)
    {
        CancellationToken = cancellationToken;
        foreach (var info in TransitionedProperties)
        {
            var value = info.Metadata.GetValue(info.Target);
            if (value is not null)
                info.InitValue = info.ManagedType.Copy(value);
        }

        Watch.Reset();
        Watch.Start();
        TransitionManager.Instance.Register(this);
    }

    internal void OnTimer()
    {
        if (CancellationToken.IsCancellationRequested)
        {
            Watch.Stop();
            Utility.RaiseEvent(TransitionCompletedEvent, this);
            return;
        }

        var completed = false;
        var percentage = 0d;
        var elapsedTime = (int)Watch.ElapsedMilliseconds;
        var transitionedProperties = new List<TransitionedPropertyInfo>();
        OTransition?.OnTimer(elapsedTime, out percentage, out completed);

        lock (Lock)
        {
            foreach (var info in TransitionedProperties)
                transitionedProperties.Add(info.Copy());
        }

        foreach (var info in transitionedProperties)
        {
            var value = info.ManagedType.IntermediateValue(info.InitValue, info.FinalValue, percentage);
            Utility.RaiseEvent(StepProperty, this);
            var args = new PropertyUpdateArgs(info.Target, info.Metadata, value);
            SetProperty(this, args);
        }

        Stream.OnNext(true);
        if (completed)
        {
            Watch.Stop();
            Utility.RaiseEvent(TransitionCompletedEvent, this);
        }
    }

    internal void RemoveProperty(TransitionedPropertyInfo info)
    {
        lock (Lock)
            TransitionedProperties.Remove(info);
    }

    private static bool IsDisposed(object target)
    {
        return target is Control controlTarget && (controlTarget.IsDisposed || controlTarget.Disposing);
    }

    private static void RegisterType(IManagedType transitionType)
    {
        var type = transitionType.ManagedType;
        ManagedTypes[type] = transitionType;
    }

    private Transition Add(object target, Metadata meta, object final)
    {
        Type type = meta.FPType;
        if (!ManagedTypes.TryGetValue(type, out var itype))
            throw new InvalidOperationException("Transition does not handle properties of type: " + meta.FPType.Name);
        var tinfo = new TransitionedPropertyInfo {
            FinalValue = final,
            Target = target,
            Metadata = meta,
            ManagedType = itype
        };
        lock (Lock)
            TransitionedProperties.Add(tinfo);
        return this;
    }

    private void SetProperty(object? sender, PropertyUpdateArgs args)
    {
        try
        {
            if (IsDisposed(args.Target) == true)
                return;

            if (args.Target is ISynchronizeInvoke invokeTarget && invokeTarget.InvokeRequired)
            {
                var asyncResult = invokeTarget.BeginInvoke(new EventHandler<PropertyUpdateArgs>(SetProperty), [sender, args]);
                asyncResult.AsyncWaitHandle.WaitOne(50);
            }
            else
                args.Metadata.SetValue(args.Target, args.Value);
        }
        catch (Exception) { }
    }
}