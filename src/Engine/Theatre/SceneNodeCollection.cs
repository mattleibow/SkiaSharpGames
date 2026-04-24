using System.Collections;

namespace SkiaSharp.Theatre;

/// <summary>
/// An ordered collection of child <see cref="SceneNode"/>s that maintains the
/// parent–child relationship. Adding a child to this collection removes it from
/// any previous parent. Implements <see cref="IList{SceneNode}"/> for full
/// collection interop.
/// </summary>
public class SceneNodeCollection : IList<SceneNode>
{
    private readonly SceneNode _parent;
    private readonly List<SceneNode> _items = [];

    internal SceneNodeCollection(SceneNode parent) => _parent = parent;

    // ── IList<SceneNode> ──────────────────────────────────────────────

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public SceneNode this[int index]
    {
        get => _items[index];
        set
        {
            var old = _items[index];
            if (ReferenceEquals(old, value))
                return;
            old.Parent = null;
            old.OnRemovedFromParent();

            value.Parent?.Children.Remove(value);
            _items[index] = value;
            value.Parent = _parent;
            value.OnAddedToParent();
        }
    }

    /// <inheritdoc />
    public void Add(SceneNode child)
    {
        child.Parent?.Children.Remove(child);
        _items.Add(child);
        child.Parent = _parent;
        child.OnAddedToParent();
        Added?.Invoke(this, new SceneNodeCollectionEventArgs { Nodes = [child] });
    }

    /// <inheritdoc />
    public bool Remove(SceneNode child)
    {
        if (!_items.Remove(child))
            return false;
        child.Parent = null;
        child.OnRemovedFromParent();
        Removed?.Invoke(this, new SceneNodeCollectionEventArgs { Nodes = [child] });
        return true;
    }

    /// <inheritdoc />
    public void Insert(int index, SceneNode child)
    {
        child.Parent?.Children.Remove(child);
        _items.Insert(index, child);
        child.Parent = _parent;
        child.OnAddedToParent();
        Added?.Invoke(this, new SceneNodeCollectionEventArgs { Nodes = [child] });
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        var child = _items[index];
        _items.RemoveAt(index);
        child.Parent = null;
        child.OnRemovedFromParent();
        Removed?.Invoke(this, new SceneNodeCollectionEventArgs { Nodes = [child] });
    }

    /// <inheritdoc />
    public int IndexOf(SceneNode child) => _items.IndexOf(child);

    /// <inheritdoc />
    public bool Contains(SceneNode child) => _items.Contains(child);

    /// <inheritdoc />
    public void Clear()
    {
        var removed = _items.ToArray();
        foreach (var child in removed)
        {
            child.Parent = null;
            child.OnRemovedFromParent();
        }
        _items.Clear();
        if (removed.Length > 0)
            Removed?.Invoke(this, new SceneNodeCollectionEventArgs { Nodes = removed });
    }

    /// <inheritdoc />
    public void CopyTo(SceneNode[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<SceneNode> GetEnumerator() => _items.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // ── Ordering (child list order = z-order) ─────────────────────────

    /// <summary>
    /// Moves <paramref name="child"/> to the end of the list (drawn last / on top).
    /// </summary>
    public void MoveToFront(SceneNode child)
    {
        if (_items.Remove(child))
            _items.Add(child);
    }

    /// <summary>
    /// Moves <paramref name="child"/> to the start of the list (drawn first / behind).
    /// </summary>
    public void MoveToBack(SceneNode child)
    {
        if (_items.Remove(child))
            _items.Insert(0, child);
    }

    /// <summary>
    /// Moves <paramref name="child"/> one position toward the end (drawn later).
    /// </summary>
    public void MoveUp(SceneNode child)
    {
        int i = _items.IndexOf(child);
        if (i >= 0 && i < _items.Count - 1)
            (_items[i], _items[i + 1]) = (_items[i + 1], _items[i]);
    }

    /// <summary>
    /// Moves <paramref name="child"/> one position toward the start (drawn earlier).
    /// </summary>
    public void MoveDown(SceneNode child)
    {
        int i = _items.IndexOf(child);
        if (i > 0)
            (_items[i], _items[i - 1]) = (_items[i - 1], _items[i]);
    }

    // ── Bulk ──────────────────────────────────────────────────────────

    /// <summary>
    /// Removes all children where <see cref="SceneNode.Active"/> is false.
    /// </summary>
    public int RemoveInactive()
    {
        var inactive = _items.Where(c => !c.Active).ToArray();
        foreach (var child in inactive)
        {
            _items.Remove(child);
            child.Parent = null;
            child.OnRemovedFromParent();
        }
        if (inactive.Length > 0)
            Removed?.Invoke(this, new SceneNodeCollectionEventArgs { Nodes = inactive });
        return inactive.Length;
    }

    // ── Events ────────────────────────────────────────────────────────

    /// <summary>Raised after one or more children are added.</summary>
    public event EventHandler<SceneNodeCollectionEventArgs>? Added;

    /// <summary>Raised after one or more children are removed.</summary>
    public event EventHandler<SceneNodeCollectionEventArgs>? Removed;
}
