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
        if (a is CircleBody c  && b is RectBody r)   return CircleRect(c, r);
        if (a is RectBody   ra && b is CircleBody cc) return CircleRect(cc, ra);
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
        float overlapLeft   = (circle.X + circle.Radius) - rect.X;
        float overlapRight  = (rect.X + rect.Width)  - (circle.X - circle.Radius);
        float overlapTop    = (circle.Y + circle.Radius) - rect.Y;
        float overlapBottom = (rect.Y + rect.Height) - (circle.Y - circle.Radius);
        float minH = MathF.Min(overlapLeft, overlapRight);
        float minV = MathF.Min(overlapTop,  overlapBottom);

        if (minV <= minH) circle.VelocityY = -circle.VelocityY;
        else              circle.VelocityX = -circle.VelocityX;
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
}
