namespace CoreOne.Winforms.Transitions;

internal interface IManagedType
{
    Type ManagedType { get; }

    object Copy(object o);

    object IntermediateValue(object start, object end, double percentage);
}