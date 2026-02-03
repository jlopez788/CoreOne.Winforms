namespace CoreOne.Winforms;

/// <summary>
/// Represents a handler that can process watch context events in a pipeline
/// </summary>
public interface IWatchFactory
{
    int Priority => 0;

    /// <summary>
    /// Creates a handler instance for the given property
    /// </summary>
    /// <param name="gridItem">The property grid item</param>
    /// <returns>A handler instance</returns>
    IWatchHandler? CreateInstance(PropertyGridItem gridItem);
}

/// <summary>
/// Represents an instance of a watch context handler that processes events for a specific property
/// </summary>
public interface IWatchHandler
{
    /// <summary>
    /// Gets the property dependencies this handler needs to watch
    /// </summary>
    HashSet<string> Dependencies { get; }
    /// <summary>
    /// Target property this handler is associated with
    /// </summary>
    Metadata Property { get; }

    /// <summary>
    /// Called when a dependent property changes
    /// </summary>
    /// <param name="model">The model being watched</param>
    void Refresh(object model);
}