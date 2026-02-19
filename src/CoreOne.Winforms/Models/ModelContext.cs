using CoreOne.Collections;
using CoreOne.Reactive;
using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Events;
using System.Reflection;

namespace CoreOne.Winforms.Models;

public class ModelContext
{
    private const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
    private readonly DataList<string, Metadata> DependencyMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Data<int, GroupDetail> Groups;
    private readonly DataList<Metadata, IWatchHandler> Registrations = [];
    private readonly Type Type;
    private ModelTransaction Transaction;
    public bool IsModified { get; private set; }
    public object Model => Transaction.Model;
    public IReadOnlyList<Metadata> Properties { get; }
    public Subject<ModelPropertyChanged> PropertyChanged { get; } = new();

    public ModelContext(object model) : this(model, model.GetType())
    {
    }

    private ModelContext(object model, Type type)
    {
        Type = type;
        Properties = [.. MetaType.GetMetadatas(type, FLAGS)
            .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<IgnoreAttribute>() is null)];
        Transaction = new ModelTransaction(model);
        Groups = new Data<int, GroupDetail> {
            DefaultKey = 0,
            [0] = GroupDetail.Default
        };
    }

    public static ModelContext Create<T>(T? model) where T : class
    {
        var type = typeof(T);
        var instanceModel = model ?? type.GetInstance();
        return new ModelContext(instanceModel, type);
    }

    public ModelContext AddGroup(GroupDetail group)
    {
        Groups.Set(group.GroupId, group);
        return this;
    }

    public ModelContext AddGroups(IEnumerable<GroupDetail> groups)
    {
        groups.Each(p => AddGroup(p));
        return this;
    }

    public void Clear()
    {
        Registrations.Clear();
        DependencyMap.Clear();
    }

    public void Commit()
    {
        IsModified = false;
        Transaction?.Commit();
    }

    public GridColumnSpan GetGridColumnSpan(int groupId) => Groups.Get(groupId)?.ColumnSpan ?? GridColumnSpan.Default;

    public GroupDetail? GetGroup(int groupId) => Groups.Get(groupId);

    public IEnumerable<GroupDetail> GetGroupDetails() => Groups.Values.OrderByDescending(g => g.Priority);

    public IEnumerable<GroupEntry> GetGroupEntries() => Properties
            .Select(p => {
                var attribute = p.GetCustomAttribute<GroupAttribute>();
                return new {
                    property = p,
                    groupId = attribute?.GroupId ?? 0
                };
            }).GroupBy(p => p.groupId, p => p,
                (groupId, p) => new GroupEntry(p.SelectList(m => m.property), groupId, Groups.Get(groupId)?.Priority ?? 0))
            .OrderByDescending(p => p.Priority);

    public override int GetHashCode() => Transaction?.Model.GetHashCode() ?? Type.GetHashCode();

    public void NotifyPropertyChanged(object model, string propertyName, object? newValue)
    {
        var kp = Registrations.FirstOrDefault(p => p.Key.Name.Matches(propertyName));
        IsModified = true;
        kp.Value?.Each(p => p.Refresh(model));

        DependencyMap.Get(propertyName)
               ?.Select(p => Registrations.TryGetValue(p, out var context) ? context : null)
               ?.SelectMany(p => p is null ? [] : p)
               ?.Each(p => p.Refresh(model));
    }

    public void RegisterContext(IWatchHandler context, object model)
    {
        Registrations.Add(context.Property, context);

        context.Refresh(model);
        context.Dependencies.Each(p => DependencyMap.Add(p, context.Property));
    }

    public void Rollback()
    {
        IsModified = false;
        Transaction.Rollback();
        Transaction = new ModelTransaction(Model);
    }

    public record GroupEntry(List<Metadata> Properties, int GroupId, int Priority);
}