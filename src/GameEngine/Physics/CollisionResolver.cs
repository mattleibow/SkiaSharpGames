namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Static helpers that resolve collisions between <see cref="PhysicsBody"/> instances.
/// Supports <see cref="CircleBody"/>/<see cref="RectBody"/> combinations for overlap detection
/// and velocity reflection.
/// </summary>
/// <remarks>
/// These methods are also exposed as instance methods on <see cref="PhysicsBody"/> for convenience:
/// <c>body.Overlaps(other)</c>, <c>body.Reflect(other)</c>, <c>body.ReflectOff(other)</c>.
/// </remarks>
public static class CollisionResolver
{
    // ── Overlap ───────────────────────────────────────────────────────────

    /// <summary>Returns <see langword="true"/> when <paramref name="a"/> overlaps <paramref name="b"/>.</summary>
    public static bool Overlaps(PhysicsBody a, PhysicsBody b)
    {
        if (a is CircleBody ca && b is CircleBody cb) return CircleCircle(ca, cb);
        if (a is CircleBody c && b is RectBody r) return CircleRect(c, r);
        if (a is RectBody ra && b is CircleBody cc) return CircleRect(cc, ra);
        return RectRect(a, b); // RectBody vs RectBody
    }

    // ── Reflect ───────────────────────────────────────────────────────────

    /// <summary>
    /// Reflects <paramref name="a"/>'s velocity off <paramref name="b"/> using the smallest-overlap axis.
    /// Only has an effect when <paramref name="a"/> is a <see cref="CircleBody"/> and
    /// <paramref name="b"/> is a <see cref="RectBody"/>; all other combinations are no-ops.
    /// Does <b>not</b> check for overlap first — use <see cref="ReflectOff"/> when you need
    /// the overlap check included.
    /// </summary>
    public static void Reflect(PhysicsBody a, PhysicsBody b)
    {
        if (a is CircleBody circle && b is RectBody rect)
            ReflectCircleOnRect(circle, rect);
    }

    // ── ReflectOff ────────────────────────────────────────────────────────

    /// <summary>
    /// If <paramref name="a"/> overlaps <paramref name="b"/>, reflects <paramref name="a"/>'s
    /// velocity and returns <see langword="true"/>. Returns <see langword="false"/> when there
    /// is no overlap.
    /// </summary>
    public static bool ReflectOff(PhysicsBody a, PhysicsBody b)
    {
        if (!Overlaps(a, b)) return false;

        if (a is CircleBody ca && b is CircleBody cb)
        {
            float dx = ca.X - cb.X, dy = ca.Y - cb.Y;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            if (len > 0f)
            {
                float nx = dx / len, ny = dy / len;
                float dot = ca.VelocityX * nx + ca.VelocityY * ny;
                ca.VelocityX -= 2f * dot * nx;
                ca.VelocityY -= 2f * dot * ny;
            }
            return true;
        }

        Reflect(a, b);
        return true;
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private static void ReflectCircleOnRect(CircleBody circle, RectBody rect)
    {
        float overlapLeft = (circle.X + circle.Radius) - rect.X;
        float overlapRight = (rect.X + rect.Width) - (circle.X - circle.Radius);
        float overlapTop = (circle.Y + circle.Radius) - rect.Y;
        float overlapBottom = (rect.Y + rect.Height) - (circle.Y - circle.Radius);
        float minH = MathF.Min(overlapLeft, overlapRight);
        float minV = MathF.Min(overlapTop, overlapBottom);

        if (minV <= minH) circle.VelocityY = -circle.VelocityY;
        else circle.VelocityX = -circle.VelocityX;
    }

    private static bool CircleCircle(CircleBody a, CircleBody b)
    {
        float dx = a.X - b.X, dy = a.Y - b.Y, r = a.Radius + b.Radius;
        return dx * dx + dy * dy < r * r;
    }

    private static bool CircleRect(CircleBody c, RectBody r)
    {
        float nearX = Math.Clamp(c.X, r.X, r.X + r.Width);
        float nearY = Math.Clamp(c.Y, r.Y, r.Y + r.Height);
        float dx = c.X - nearX, dy = c.Y - nearY;
        return dx * dx + dy * dy < c.Radius * c.Radius;
    }

    private static bool RectRect(PhysicsBody a, PhysicsBody b)
        => a.BoundingBox.IntersectsWith(b.BoundingBox);

    // ── Wall resolution ───────────────────────────────────────────────────

    /// <summary>
    /// Resolves left-wall, right-wall, and ceiling collisions for a circle moving through a bounded
    /// game area. The game area is assumed to span from <c>X=0</c> to <c>X=gameWidth</c> and from
    /// <c>Y=0</c> (ceiling) downward. The bottom boundary is intentionally excluded — treat that
    /// as an out-of-bounds condition in the caller.
    /// </summary>
    public static void ResolveWalls(Entity entity, CircleCollider collider, Rigidbody2D rigidbody, float gameWidth)
    {
        float r = collider.Radius;

        if (entity.X - r < 0f)
        {
            entity.X = r;
            rigidbody.VelocityX = MathF.Abs(rigidbody.VelocityX);
        }
        else if (entity.X + r > gameWidth)
        {
            entity.X = gameWidth - r;
            rigidbody.VelocityX = -MathF.Abs(rigidbody.VelocityX);
        }

        if (entity.Y - r < 0f)
        {
            entity.Y = r;
            rigidbody.VelocityY = MathF.Abs(rigidbody.VelocityY);
        }
    }

    // ── Entity + Collider API ─────────────────────────────────────────────

    /// <summary>
    /// Returns <see langword="true"/> when the circle described by
    /// <paramref name="circle"/>/<paramref name="circleOwner"/> overlaps the rectangle described by
    /// <paramref name="rect"/>/<paramref name="rectOwner"/>.
    /// </summary>
    public static bool Overlaps(Entity circleOwner, CircleCollider circle,
                                Entity rectOwner,   RectCollider rect)
    {
        var (cx, cy) = circle.WorldCenter(circleOwner);
        var bounds   = rect.WorldRect(rectOwner);
        float nearX  = Math.Clamp(cx, bounds.Left, bounds.Right);
        float nearY  = Math.Clamp(cy, bounds.Top,  bounds.Bottom);
        float dx     = cx - nearX, dy = cy - nearY;
        return dx * dx + dy * dy < circle.Radius * circle.Radius;
    }

    /// <summary>
    /// Reflects <paramref name="rigidbody"/>'s velocity off the rectangle
    /// <paramref name="rect"/>/<paramref name="rectOwner"/> using the smallest-overlap axis.
    /// Does <b>not</b> check for overlap first.
    /// </summary>
    public static void Reflect(Entity circleOwner, CircleCollider circle, Rigidbody2D rigidbody,
                               Entity rectOwner,   RectCollider rect)
    {
        var (cx, cy) = circle.WorldCenter(circleOwner);
        float r      = circle.Radius;
        var bounds   = rect.WorldRect(rectOwner);

        float overlapLeft   = (cx + r) - bounds.Left;
        float overlapRight  = bounds.Right  - (cx - r);
        float overlapTop    = (cy + r) - bounds.Top;
        float overlapBottom = bounds.Bottom - (cy - r);
        float minH = MathF.Min(overlapLeft,  overlapRight);
        float minV = MathF.Min(overlapTop,   overlapBottom);

        if (minV <= minH) rigidbody.VelocityY = -rigidbody.VelocityY;
        else              rigidbody.VelocityX = -rigidbody.VelocityX;
    }

    /// <summary>
    /// If the circle overlaps the rectangle, reflects <paramref name="rigidbody"/>'s velocity
    /// and returns <see langword="true"/>. Returns <see langword="false"/> when there is no overlap.
    /// </summary>
    public static bool ReflectOff(Entity circleOwner, CircleCollider circle, Rigidbody2D rigidbody,
                                  Entity rectOwner,   RectCollider rect)
    {
        if (!Overlaps(circleOwner, circle, rectOwner, rect)) return false;
        Reflect(circleOwner, circle, rigidbody, rectOwner, rect);
        return true;
    }
}
