namespace SkiaSharp.Theatre;

/// <summary>
/// Event data for <see cref="SceneNodeCollection.Added"/> and
/// <see cref="SceneNodeCollection.Removed"/> events.
/// </summary>
public class SceneNodeCollectionEventArgs : EventArgs
{
    /// <summary>The nodes that were added or removed.</summary>
    public required IReadOnlyList<SceneNode> Nodes { get; init; }
}