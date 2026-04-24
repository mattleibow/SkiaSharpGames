namespace SkiaSharp.Theatre;

/// <summary>
/// Base class for all visible game objects (actors). An actor carries position
/// (with optional rotation), nullable rendering/physics/collision components.
/// Actors are nodes in the scene tree (<see cref="SceneNode"/>), with transforms
/// that cascade to children.
/// </summary>
/// <remarks>
/// Transforms are stored as <see cref="SKMatrix44"/> matrices. The
/// <see cref="LocalMatrix"/> is built from <see cref="X"/>, <see cref="Y"/>,
/// and <see cref="Rotation"/>, and the <see cref="WorldMatrix"/> is the
/// concatenation of all ancestor local matrices.
/// </remarks>
public class Actor : SceneNode
{
    private float _x;
    private float _y;
    private float _rotation;
    private SKMatrix44 _worldMatrix = SKMatrix44.Identity;

    // ── Local position / rotation ─────────────────────────────────────

    /// <summary>
    /// Horizontal position in local space (relative to parent, or world if root).
    /// </summary>
    public float X
    {
        get => _x;
        set
        {
            _x = value;
            RecalculateWorld();
        }
    }

    /// <summary>Vertical position in local space (relative to parent, or world if root).</summary>
    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            RecalculateWorld();
        }
    }

    /// <summary>Local rotation in radians. Positive = clockwise.</summary>
    public float Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            RecalculateWorld();
        }
    }

    /// <summary>When <see langword="false"/> the actor is not drawn (but still updated).</summary>
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
        var parentActor = FindParentActor();
        _worldMatrix = parentActor is null
            ? LocalMatrix
            : SKMatrix44.Concat(LocalMatrix, parentActor._worldMatrix);

        // Recurse into children that are also Actors
        var children = Children;
        for (int i = 0; i < children.Count; i++)
            if (children[i] is Actor childActor)
                childActor.RecalculateWorld();
    }

    private Actor? FindParentActor()
    {
        var node = Parent;
        while (node is not null)
        {
            if (node is Actor actor)
                return actor;
            node = node.Parent;
        }
        return null;
    }

    /// <inheritdoc/>
    internal override void OnAddedToParent() => RecalculateWorld();

    /// <inheritdoc/>
    internal override void OnRemovedFromParent() => RecalculateWorld();

    // ── Components (nullable, opt-in) ─────────────────────────────────

    /// <summary>Opacity from 0 (invisible) to 1 (fully opaque).</summary>
    public float Alpha { get; set; } = 1f;

    // Cached paint for SaveLayer alpha compositing — avoids per-frame allocation.
    private readonly SKPaint _layerPaint = new();

    /// <summary>
    /// Collision shape. Used by <see cref="WorldBoundingBox"/> and collision helpers.
    /// </summary>
    public Collider2D? Collider { get; set; }

    /// <summary>Velocity-driven movement. Stepped automatically by <see cref="Update"/>.</summary>
    public Rigidbody2D? Rigidbody { get; set; }

    // ── Update (recursive) ────────────────────────────────────────────

    /// <summary>
    /// Advances the actor tree. Steps rigidbody, calls
    /// <see cref="OnUpdate"/>, then recurses into active children.
    /// </summary>
    public override void Update(float deltaTime)
    {
        if (!Active)
            return;

        Rigidbody?.Step(this, deltaTime);
        OnUpdate(deltaTime);

        var children = Children;
        for (int i = 0; i < children.Count; i++)
            children[i].Update(deltaTime);
    }

    // ── Draw (recursive) ──────────────────────────────────────────────

    /// <summary>
    /// Renders the actor tree. Translates (and optionally rotates) the canvas,
    /// calls <see cref="OnDraw"/>, then recurses into visible children.
    /// When <see cref="Alpha"/> is less than 1, a compositing layer is used so
    /// that the opacity cascades correctly to all children.
    /// </summary>
    public override void Draw(SKCanvas canvas)
    {
        if (!Active || !Visible || Alpha <= 0f)
            return;

        if (Alpha < 1f)
        {
            // SaveLayer composites everything drawn within at the given alpha.
            // This gives correct cascading: parent 50% + child 50% = 25%.
            _layerPaint.Color = SKColors.White.WithAlpha((byte)(255 * Alpha));
            canvas.SaveLayer(_layerPaint);
        }
        else
        {
            canvas.Save();
        }

        canvas.Concat(LocalMatrix);

        OnDraw(canvas);

        var children = Children;
        for (int i = 0; i < children.Count; i++)
            children[i].Draw(canvas);

        canvas.Restore();
    }

    // ── Collision (world-space) ───────────────────────────────────────

    /// <summary>
    /// World-space bounding box. Returns a conservative axis-aligned box that
    /// encloses the collider, accounting for the full world transform. Null if no collider.
    /// </summary>
    public SKRect? WorldBoundingBox
    {
        get
        {
            if (Collider is null)
                return null;
            var local = Collider.BoundingBox(0f, 0f);

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

    /// <summary>Tests overlap with another actor in world space.</summary>
    public bool Overlaps(Actor other) => CollisionResolver.Overlaps(this, other);

    /// <summary>Tests overlap and returns collision details for bounce resolution.</summary>
    public bool TryGetHit(Actor other, out CollisionHit hit) =>
        CollisionResolver.TryGetHit(this, other, out hit);

    /// <summary>
    /// If overlapping, bounces this actor's rigidbody off <paramref name="other"/>.
    /// </summary>
    public bool BounceOff(Actor other)
    {
        if (Rigidbody is null || !TryGetHit(other, out var hit))
            return false;
        Rigidbody.Bounce(hit);
        return true;
    }

    // ── Child queries ─────────────────────────────────────────────────

    /// <summary>
    /// Aggregate world-space bounding box of all active Actor children with colliders.
    /// </summary>
    public SKRect? ChildrenBoundingBox
    {
        get
        {
            var children = Children;
            if (children.Count == 0)
                return null;
            SKRect? result = null;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is not Actor c || !c.Active)
                    continue;
                var bb = c.WorldBoundingBox;
                if (bb is null)
                    continue;
                result = result is null ? bb.Value : SKRect.Union(result.Value, bb.Value);
            }
            return result;
        }
    }

    /// <summary>
    /// Finds the first active Actor child that collides with <paramref name="other"/>.
    /// Performs a broad-phase group bounding box check first.
    /// </summary>
    public Actor? FindChildCollision(Actor other, out CollisionHit hit)
    {
        var children = Children;
        if (children.Count == 0 || other.Collider is null)
        {
            hit = default;
            return null;
        }

        var groupBB = ChildrenBoundingBox;
        var otherBB = other.WorldBoundingBox;
        if (groupBB is null || otherBB is null || !groupBB.Value.IntersectsWith(otherBB.Value))
        {
            hit = default;
            return null;
        }

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is not Actor c || !c.Active || c.Collider is null)
                continue;
            if (CollisionResolver.TryGetHit(other, c, out hit))
                return c;
        }

        hit = default;
        return null;
    }

    // ── Debug / inspection ───────────────────────────────────────────

    /// <summary>
    /// Returns a human-readable tree representation of this actor and all descendants.
    /// Useful for debugging and test assertions.
    /// </summary>
    /// <param name="indent">Current indentation prefix (used for recursion).</param>
    public new string Dump(string indent = "")
    {
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var sb = new System.Text.StringBuilder();
        sb.Append(indent).Append(GetType().Name);
        if (!string.IsNullOrEmpty(Name))
            sb.Append(" '").Append(Name).Append('\'');
        sb.Append(" @ (")
            .Append(X.ToString("F1", inv))
            .Append(", ")
            .Append(Y.ToString("F1", inv))
            .Append(')')
            .Append(" world=(")
            .Append(WorldX.ToString("F1", inv))
            .Append(", ")
            .Append(WorldY.ToString("F1", inv))
            .Append(')')
            .Append(" a=")
            .Append(Alpha.ToString("F2", inv));
        if (!Active)
            sb.Append(" INACTIVE");
        if (!Visible)
            sb.Append(" HIDDEN");
        if (Rotation != 0f)
            sb.Append(" rot=").Append(Rotation.ToString("F2", inv));
        sb.AppendLine();

        if (Collider is RectCollider rc)
            sb.Append(indent)
                .Append("  collider: Rect ")
                .Append(rc.Width.ToString("F0", inv))
                .Append('x')
                .Append(rc.Height.ToString("F0", inv))
                .AppendLine();
        else if (Collider is CircleCollider cc)
            sb.Append(indent)
                .Append("  collider: Circle r=")
                .Append(cc.Radius.ToString("F0", inv))
                .AppendLine();

        if (Rigidbody is { } rb && (rb.VelocityX != 0f || rb.VelocityY != 0f))
            sb.Append(indent)
                .Append("  velocity: (")
                .Append(rb.VelocityX.ToString("F1", inv))
                .Append(", ")
                .Append(rb.VelocityY.ToString("F1", inv))
                .Append(')')
                .AppendLine();

        var children = Children;
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is Actor childActor)
                sb.Append(childActor.Dump(indent + "  "));
            else
                sb.Append(children[i].Dump(indent + "  "));
        }

        return sb.ToString();
    }
}
