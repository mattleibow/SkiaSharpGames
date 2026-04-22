namespace SkiaSharp.Theatre;

/// <summary>
/// Base class for all nodes in the scene tree. Both <see cref="Actor"/> (visible game
/// objects) and <see cref="Scene"/> (game scenes) derive from this class, gaining a
/// shared parent/child hierarchy, recursive update/draw, and active state.
/// </summary>
public class SceneNode
{
    private List<SceneNode>? _children;

    /// <summary>Optional human-readable name for debugging and lookup.</summary>
    public string? Name { get; set; }

    /// <summary>
    /// When <see langword="false"/> the node is skipped during update and rendering.
    /// </summary>
    public bool Active { get; set; } = true;

    // ── Children ──────────────────────────────────────────────────────

    /// <summary>Parent node, or null if this is a root node.</summary>
    public SceneNode? Parent { get; private set; }

    /// <summary>Ordered list of child nodes.</summary>
    public IReadOnlyList<SceneNode> Children => (IReadOnlyList<SceneNode>?)_children ?? [];

    /// <summary>Number of children.</summary>
    public int ChildCount => _children?.Count ?? 0;

    /// <summary>
    /// Adds <paramref name="child"/> to this node's children. If the child already
    /// has a parent it is removed from that parent first.
    /// </summary>
    public void AddChild(SceneNode child)
    {
        child.Parent?.RemoveChild(child);
        (_children ??= []).Add(child);
        child.Parent = this;
        child.OnAddedToParent();
    }

    /// <summary>Removes <paramref name="child"/> from this node's children.</summary>
    public void RemoveChild(SceneNode child)
    {
        if (_children?.Remove(child) == true)
        {
            child.Parent = null;
            child.OnRemovedFromParent();
        }
    }

    /// <summary>Removes all children where <see cref="Active"/> is false.</summary>
    public int RemoveInactiveChildren()
    {
        if (_children is null) return 0;
        return _children.RemoveAll(c =>
        {
            if (c.Active) return false;
            c.Parent = null;
            c.OnRemovedFromParent();
            return true;
        });
    }

    /// <summary>Inserts <paramref name="child"/> at the given index.</summary>
    public void InsertChild(int index, SceneNode child)
    {
        child.Parent?.RemoveChild(child);
        (_children ??= []).Insert(index, child);
        child.Parent = this;
        child.OnAddedToParent();
    }

    /// <summary>Returns the index of <paramref name="child"/>, or -1 if not found.</summary>
    public int IndexOf(SceneNode child) => _children?.IndexOf(child) ?? -1;

    /// <summary>
    /// Moves <paramref name="child"/> to the end of the children list (drawn last / on top).
    /// </summary>
    public void MoveChildToFront(SceneNode child)
    {
        if (_children is null || !_children.Remove(child)) return;
        _children.Add(child);
    }

    /// <summary>
    /// Moves <paramref name="child"/> to the start of the children list (drawn first / behind).
    /// </summary>
    public void MoveChildToBack(SceneNode child)
    {
        if (_children is null || !_children.Remove(child)) return;
        _children.Insert(0, child);
    }

    /// <summary>
    /// Moves <paramref name="child"/> one position toward the end (drawn later).
    /// </summary>
    public void MoveChildUp(SceneNode child)
    {
        if (_children is null) return;
        int idx = _children.IndexOf(child);
        if (idx < 0 || idx >= _children.Count - 1) return;
        (_children[idx], _children[idx + 1]) = (_children[idx + 1], _children[idx]);
    }

    /// <summary>
    /// Moves <paramref name="child"/> one position toward the start (drawn earlier).
    /// </summary>
    public void MoveChildDown(SceneNode child)
    {
        if (_children is null) return;
        int idx = _children.IndexOf(child);
        if (idx <= 0) return;
        (_children[idx], _children[idx - 1]) = (_children[idx - 1], _children[idx]);
    }

    // ── Update (recursive) ────────────────────────────────────────────

    /// <summary>
    /// Advances the node tree. Calls <see cref="OnUpdate"/>, then recurses into active children.
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        if (!Active) return;

        OnUpdate(deltaTime);

        if (_children is not null)
            for (int i = 0; i < _children.Count; i++)
                _children[i].Update(deltaTime);
    }

    /// <summary>Override for node-specific per-frame logic.</summary>
    protected virtual void OnUpdate(float deltaTime) { }

    // ── Draw (recursive) ──────────────────────────────────────────────

    /// <summary>
    /// Renders the node tree. Calls <see cref="OnDraw"/>, then recurses into active children.
    /// </summary>
    public virtual void Draw(SkiaSharp.SKCanvas canvas)
    {
        if (!Active) return;

        OnDraw(canvas);

        if (_children is not null)
            for (int i = 0; i < _children.Count; i++)
                _children[i].Draw(canvas);
    }

    /// <summary>Override to render the node.</summary>
    protected virtual void OnDraw(SkiaSharp.SKCanvas canvas) { }

    /// <summary>
    /// Called when this node is added to a parent. Override to react to reparenting.
    /// </summary>
    protected virtual void OnAddedToParent() { }

    /// <summary>Called when this node is removed from a parent.</summary>
    protected virtual void OnRemovedFromParent() { }

    // ── Debug / inspection ───────────────────────────────────────────

    /// <summary>
    /// Returns a human-readable tree representation of this node and all descendants.
    /// </summary>
    public string Dump(string indent = "")
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(indent).Append(GetType().Name);
        if (!string.IsNullOrEmpty(Name)) sb.Append(" '").Append(Name).Append('\'');
        if (!Active) sb.Append(" INACTIVE");
        sb.AppendLine();

        if (_children is not null)
            for (int i = 0; i < _children.Count; i++)
                sb.Append(_children[i].Dump(indent + "  "));

        return sb.ToString();
    }
}
