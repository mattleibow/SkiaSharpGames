namespace SkiaSharp.Theatre;

/// <summary>
/// Base class for all nodes in the scene tree. Both <see cref="Actor"/> (visible game
/// objects) and <see cref="Scene"/> (game scenes) derive from this class, gaining a
/// shared parent/child hierarchy, recursive update/draw, active state, and theme
/// resolution.
/// </summary>
public class SceneNode
{
    /// <summary>Optional human-readable name for debugging and lookup.</summary>
    public string? Name { get; set; }

    /// <summary>
    /// When <see langword="false"/> the node is skipped during update and rendering.
    /// </summary>
    public bool Active { get; set; } = true;

    // ── Children ──────────────────────────────────────────────────────

    /// <summary>Parent node, or null if this is a root node.</summary>
    public SceneNode? Parent { get; internal set; }

    /// <summary>Ordered collection of child nodes.</summary>
    public SceneNodeCollection Children { get; }

    /// <summary>Number of children.</summary>
    public int ChildCount => Children.Count;

    public SceneNode()
    {
        Children = new SceneNodeCollection(this);
    }

    // ── Theme ─────────────────────────────────────────────────────────

    /// <summary>
    /// Optional theme override for this node. When set, this node and its descendants
    /// will use this theme instead of inheriting from the parent.
    /// </summary>
    public HudTheme? HudTheme { get; set; }

    /// <summary>
    /// The effective theme for this node, resolved by walking up the tree.
    /// </summary>
    public virtual HudTheme? ResolvedHudTheme => HudTheme ?? Parent?.ResolvedHudTheme;

    // ── Update (recursive) ────────────────────────────────────────────

    /// <summary>
    /// Advances the node tree. Calls <see cref="OnUpdate"/>, then recurses into active children.
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        if (!Active)
            return;

        OnUpdate(deltaTime);

        for (int i = 0; i < Children.Count; i++)
            Children[i].Update(deltaTime);
    }

    /// <summary>Override for node-specific per-frame logic.</summary>
    protected virtual void OnUpdate(float deltaTime) { }

    // ── Draw (recursive) ──────────────────────────────────────────────

    /// <summary>
    /// Renders the node tree. Calls <see cref="OnDraw"/>, then recurses into active children.
    /// </summary>
    public virtual void Draw(SKCanvas canvas)
    {
        if (!Active)
            return;

        OnDraw(canvas);

        for (int i = 0; i < Children.Count; i++)
            Children[i].Draw(canvas);
    }

    /// <summary>Override to render the node.</summary>
    protected virtual void OnDraw(SKCanvas canvas) { }

    /// <summary>
    /// Called when this node is added to a parent. Override to react to reparenting.
    /// </summary>
    internal virtual void OnAddedToParent() { }

    /// <summary>Called when this node is removed from a parent.</summary>
    internal virtual void OnRemovedFromParent() { }

    // ── Debug / inspection ───────────────────────────────────────────

    /// <summary>
    /// Returns a human-readable tree representation of this node and all descendants.
    /// </summary>
    public string Dump(string indent = "")
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(indent).Append(GetType().Name);
        if (!string.IsNullOrEmpty(Name))
            sb.Append(" '").Append(Name).Append('\'');
        if (!Active)
            sb.Append(" INACTIVE");
        sb.AppendLine();

        for (int i = 0; i < Children.Count; i++)
            sb.Append(Children[i].Dump(indent + "  "));

        return sb.ToString();
    }
}