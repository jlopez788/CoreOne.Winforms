namespace CoreOne.Winforms;

public interface IRefreshManager
{
    void Clear();

    void NotifyPropertyChanged(object model, string propertyName, object? newValue);

    void RegisterContext(WatchContext context, object model);
}