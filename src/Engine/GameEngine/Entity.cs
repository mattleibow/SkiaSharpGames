using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Base class for all game objects. An entity carries position (with optional
/// rotation), nullable rendering/physics/collision components, and an ordered
/// list of children. Children store local-space positions relative to their
/// parent; world positions are cached and recalculated whenever any ancestor's
/// local position or rotation changes.
/// </summary>
/// <remarks>
/// Transforms are stored as <see cref="SKMatrix44"/> matrices. The
/// <see cref="LocalMatrix"/> is built from <see cref="X"/>, <see cref="Y"/>,
/// and <see cref="Rotation"/>, and the <see cref="WorldMatrix"/> is the
/// concatenation of all ancestor local matrices.
/// </remarks>
public class Entity
{
    private float _x, _y;
    private float _rotation;
    private SKMatrix44 _worldMatrix = SKMatrix44.Identity;
    private List<Entity>? _children;

    // ── Local position / rotation ─────────────────────────────────────

    /// <summary>Horizontal position in local space (relative to parent, or world if root).</summary>
    public float X
    {
        get => _x;
        set { _x = value; RecalculateWorld(); }
    }

    /// <summary>Vertical position in local space (relative to parent, or world if root).</summary>
    public float Y
    {
        get => _y;
        set { _y = value; RecalculateWorld(); }
    }

    /// <summary>Local rotation in radians. Positive = clockwise.</summary>
    public float Rotation
    {
        get => _rotation;
        set { _rotation = value; RecalculateWorld(); }
    }

    /// <summary>When <see langword="false"/> the entity is skipped during update and rendering.</summary>
    public bool Active { get; set; } = true;

    /// <summary>When <see langword="false"/> the entity is not drawn (but still updated).</summary>
    public bool Visible { get; set; } = true;

    // ── Transform matrices ────────────────────────────────────────────

    /// <summary>
    /// The local transform matrix built from <see cref="X"/>, <see cref="Y"/>,
    /// and <see cref="Rotation"/>. Composed as Translate × RotZ.
    /// </summary>
    public SKMatrix44 LocalMatrix
    {
        get
        {
            var matrix = SKMatrix44.CreateTranslation(_x, _y, 0f);
            if (_rotation != 0f)
                matrix = matrix.PostConcat(SKMatrix44.CreateRotation(0f, 0f, 1f, _rotation));
            return matrix;
        }
    }

    /// <summary>
    /// The accumulated world transform matrix, accounting for all ancestor local matrices.
    /// </summary>
    public SKMatrix44 WorldMatrix => _worldMatrix;

    // ── World transform (derived from WorldMatrix) ────────────────────

    /// <summary>World-space X, accounting for all ancestor positions and rotations.</summary>
    public float WorldX => _worldMatrix.MapPoint(0f, 0f).X;

    /// <summary>World-space Y, accounting for all ancestor positions and rotations.</summary>
    public float WorldY => _worldMatrix.MapPoint(0f, 0f).Y;

    private void RecalculateWorld()
    {
        _worldMatrix = Parent is null
            ? LocalMatrix
            : SKMatrix44.Concat(LocalMatrix, Parent._worldMatrix);

        if (_children is not null)
            for (int i = 0; i < _children.Count; i++)
                _children[i].RecalculateWorld();
    }

    // ── Components (nullable, opt-in) ─────────────────────────────────

    /// <summary>Opacity from 0 (invisible) to 1 (fully opaque).</summary>
    public float Alpha { get; set; } = 1f;

    /// <summary>Collision shape. Used by <see cref="WorldBoundingBox"/> and collision helpers.</summary>
    public Collider2D? Collider { get; set; }

    /// <summary>Velocity-driven movement. Stepped automatically by <see cref="Update"/>.</summary>
    public Rigidbody2D? Rigidbody { get; set; }

    // ── Children ──────────────────────────────────────────────────────

    /// <summary>Parent entity, or null if this is a root entity.</summary>
    public Entity? Parent { get; private set; }

    /// <summary>Ordered list of child entities.</summary>
    public IReadOnlyList<Entity> Children => (IReadOnlyList<Entity>?)_children ?? [];

    /// <summary>Number of children.</summary>
    public int ChildCount => _children?.Count ?? 0;

    /// <summary>
    /// Adds <paramref name="child"/> to this entity's children. If the child already
    /// has a parent it is removed from that parent first. World positions are recalculated.
    /// </summary>
    public void AddChild(Entity child)
    {
        child.Parent?.RemoveChild(child);
        (_children ??= []).Add(child);
        child.Parent = this;
        child.RecalculateWorld();
    }

    /// <summary>Removes <paramref name="child"/> from this entity's children.</summary>
    public void RemoveChild(Entity child)
    {
        if (_children?.Remove(child) == true)
        {
            child.Parent = null;
            child.RecalculateWorld();
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
            c.RecalculateWorld();
            return true;
        });
    }

    // ── Update (recursive) ────────────────────────────────────────────

    /// <summary>
    /// Advances the entity tree. Steps rigidbody, calls
    /// <see cref="OnUpdate"/>, then recurses into active children.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!Active) return;

        Rigidbody?.Step(this, deltaTime);
        OnUpdate(deltaTime);

        if (_children is not null)
            for (int i = 0; i < _children.Count; i++)
                _children[i].Update(deltaTime);
    }

    /// <summary>Override for game-specific per-frame logic (timers, AI, etc.).</summary>
    protected virtual void OnUpdate(float deltaTime) { }

    // ── Draw (recursive) ──────────────────────────────────────────────

    /// <summary>
    /// Renders the entity tree. Translates (and optionally rotates) the canvas,
    /// calls <see cref="OnDraw"/>, then recurses into visible children.
    /// </summary>
    public void Draw(SKCanvas canvas)
    {
        if (!Active || !Visible) return;

        canvas.Save();
        canvas.Concat(LocalMatrix);

        OnDraw(canvas);

        if (_children is not null)
            for (int i = 0; i < _children.Count; i++)
                _children[i].Draw(canvas);

        canvas.Restore();
    }

    /// <summary>Override to render the entity at the local origin. The canvas
    /// is already translated to the entity's position.</summary>
    protected virtual void OnDraw(SKCanvas canvas) { }

    // ── Collision (world-space) ───────────────────────────────────────

    /// <summary>
    /// World-space bounding box. Returns a conservative axis-aligned box that
    /// encloses the collider, accounting for the full world transform. Null if no collider.
    /// </summary>
    public SKRect? WorldBoundingBox
    {
        get
        {
            if (Collider is null) return null;
            var local = Collider.BoundingBox(0f, 0f);

            // Map the four corners through the world matrix and find the enclosing AABB
            var m = _worldMatrix;
            var p1 = m.MapPoint(local.Left, local.Top);
            var p2 = m.MapPoint(local.Right, local.Top);
            var p3 = m.MapPoint(local.Right, local.Bottom);
            var p4 = m.MapPoint(local.Left, local.Bottom);

            float minX = MathF.Min(MathF.Min(p1.X, p2.X), MathF.Min(p3.X, p4.X));
            float minY = MathF.Min(MathF.Min(p1.Y, p2.Y), MathF.Min(p3.Y, p4.Y));
            float maxX = MathF.Max(MathF.Max(p1.X, p2.X), MathF.Max(p3.X, p4.X));
            float maxY = MathF.Max(MathF.Max(p1.Y, p2.Y), MathF.Max(p3.Y, p4.Y));

            return new SKRect(minX, minY, maxX, maxY);
        }
    }

    /// <summary>Tests overlap with another entity in world space.</summary>
    public bool Overlaps(Entity other) =>
        CollisionResolver.Overlaps(this, other);

    /// <summary>Tests overlap and returns collision details for bounce resolution.</summary>
    public bool TryGetHit(Entity other, out CollisionHit hit) =>
        CollisionResolver.TryGetHit(this, other, out hit);

    /// <summary>If overlapping, bounces this entity's rigidbody off <paramref name="other"/>.</summary>
    public bool BounceOff(Entity other)
    {
        if (Rigidbody is null || !TryGetHit(other, out var hit)) return false;
        Rigidbody.Bounce(hit);
        return true;
    }

    // ── Child queries ─────────────────────────────────────────────────

    /// <summary>Aggregate world-space bounding box of all active children with colliders.</summary>
    public SKRect? ChildrenBoundingBox
    {
        get
        {
            if (_children is null) return null;
            SKRect? result = null;
            for (int i = 0; i < _children.Count; i++)
            {
                var c = _children[i];
                if (!c.Active) continue;
                var bb = c.WorldBoundingBox;
                if (bb is null) continue;
                result = result is null ? bb.Value : SKRect.Union(result.Value, bb.Value);
            }
            return result;
        }
    }

    /// <summary>
    /// Finds the first active child that collides with <paramref name="other"/>.
    /// Performs a broad-phase group bounding box check first.
    /// </summary>
    public Entity? FindChildCollision(Entity other, out CollisionHit hit)
    {
        if (_children is null || other.Collider is null)
        {
            hit = default;
            return null;
        }

        // Broad-phase: skip all children if other doesn't overlap group bounds
        var groupBB = ChildrenBoundingBox;
        var otherBB = other.WorldBoundingBox;
        if (groupBB is null || otherBB is null
            || !groupBB.Value.IntersectsWith(otherBB.Value))
        {
            hit = default;
            return null;
        }

        // Narrow-phase: test each active child
        for (int i = 0; i < _children.Count; i++)
        {
            var c = _children[i];
            if (!c.Active || c.Collider is null) continue;
            if (CollisionResolver.TryGetHit(other, c, out hit))
                return c;
        }

        hit = default;
        return null;
    }

    // ── Debug / inspection ───────────────────────────────────────────

    /// <summary>
    /// Returns a human-readable tree representation of this entity and all descendants.
    /// Useful for debugging and test assertions.
    /// </summary>
    /// <param name="indent">Current indentation prefix (used for recursion).</param>
    public string Dump(string indent = "")
    {
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var sb = new System.Text.StringBuilder();
        sb.Append(indent).Append(GetType().Name)
          .Append(" @ (").Append(X.ToString("F1", inv)).Append(", ").Append(Y.ToString("F1", inv)).Append(')')
          .Append(" world=(").Append(WorldX.ToString("F1", inv)).Append(", ").Append(WorldY.ToString("F1", inv)).Append(')')
          .Append(" a=").Append(Alpha.ToString("F2", inv));
        if (!Active) sb.Append(" INACTIVE");
        if (!Visible) sb.Append(" HIDDEN");
        if (Rotation != 0f) sb.Append(" rot=").Append(Rotation.ToString("F2", inv));
        sb.AppendLine();

        if (Collider is RectCollider rc)
            sb.Append(indent).Append("  collider: Rect ").Append(rc.Width.ToString("F0", inv)).Append('x').Append(rc.Height.ToString("F0", inv)).AppendLine();
        else if (Collider is CircleCollider cc)
            sb.Append(indent).Append("  collider: Circle r=").Append(cc.Radius.ToString("F0", inv)).AppendLine();

        if (Rigidbody is { } rb && (rb.VelocityX != 0f || rb.VelocityY != 0f))
            sb.Append(indent).Append("  velocity: (").Append(rb.VelocityX.ToString("F1", inv)).Append(", ").Append(rb.VelocityY.ToString("F1", inv)).Append(')').AppendLine();

        if (_children is not null)
            for (int i = 0; i < _children.Count; i++)
                sb.Append(_children[i].Dump(indent + "  "));

        return sb.ToString();
    }
}
