namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Static helpers for simple arcade-style 2D collisions between entities.
/// </summary>
public static class CollisionResolver
{
    // ── Entity API (uses WorldX/WorldY and Collider) ──────────────────

    /// <summary>
    /// Returns <see langword="true"/> when the two entities' colliders overlap
    /// in world space. Returns false if either entity has no collider.
    /// </summary>
    public static bool Overlaps(Entity a, Entity b)
    {
        if (a.Collider is null || b.Collider is null) return false;
        return TryGetHit(a.WorldX, a.WorldY, a.Collider,
                         b.WorldX, b.WorldY, b.Collider, out _);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the two entities overlap and outputs
    /// <paramref name="hit"/> with the collision normal and penetration.
    /// Returns false if either entity has no collider.
    /// </summary>
    public static bool TryGetHit(Entity a, Entity b, out CollisionHit hit)
    {
        if (a.Collider is null || b.Collider is null)
        {
            hit = default;
            return false;
        }
        return TryGetHit(a.WorldX, a.WorldY, a.Collider,
                         b.WorldX, b.WorldY, b.Collider, out hit);
    }

    // ── Position-based API ────────────────────────────────────────────

    /// <summary>
    /// Tries to compute collision information for two colliders at explicit positions.
    /// </summary>
    public static bool TryGetHit(float ax, float ay, Collider2D colliderA,
                                 float bx, float by, Collider2D colliderB,
                                 out CollisionHit hit)
    {
        switch (colliderA)
        {
            case CircleCollider circleA when colliderB is CircleCollider circleB:
                return TryGetCircleCircleHit(ax, ay, circleA, bx, by, circleB, out hit);

            case CircleCollider circle when colliderB is RectCollider rect:
                return TryGetCircleRectHit(ax, ay, circle, bx, by, rect, out hit);

            case RectCollider rectA when colliderB is CircleCollider circleB:
                if (TryGetCircleRectHit(bx, by, circleB, ax, ay, rectA, out var reverseHit))
                {
                    hit = new CollisionHit(-reverseHit.NormalX, -reverseHit.NormalY, reverseHit.Penetration);
                    return true;
                }
                hit = default;
                return false;

            case RectCollider rectA when colliderB is RectCollider rectB:
                return TryGetRectRectHit(ax, ay, rectA, bx, by, rectB, out hit);

            default:
                throw new NotSupportedException($"Unsupported collider pair: {colliderA.GetType().Name} and {colliderB.GetType().Name}.");
        }
    }

    private static bool TryGetCircleCircleHit(float ax, float ay, CircleCollider circleA,
                                               float bx, float by, CircleCollider circleB,
                                               out CollisionHit hit)
    {
        var (cax, cay) = circleA.WorldCenter(ax, ay);
        var (cbx, cby) = circleB.WorldCenter(bx, by);
        float dx = cax - cbx;
        float dy = cay - cby;
        float distanceSquared = dx * dx + dy * dy;
        float radius = circleA.Radius + circleB.Radius;
        float radiusSquared = radius * radius;

        if (distanceSquared >= radiusSquared)
        {
            hit = default;
            return false;
        }

        if (distanceSquared <= 0f)
        {
            hit = new CollisionHit(0f, -1f, radius);
            return true;
        }

        float distance = MathF.Sqrt(distanceSquared);
        hit = new CollisionHit(dx / distance, dy / distance, radius - distance);
        return true;
    }

    private static bool TryGetRectRectHit(float ax, float ay, RectCollider rectA,
                                           float bx, float by, RectCollider rectB,
                                           out CollisionHit hit)
    {
        var aBounds = rectA.WorldRect(ax, ay);
        var bBounds = rectB.WorldRect(bx, by);

        if (!aBounds.IntersectsWith(bBounds))
        {
            hit = default;
            return false;
        }

        float overlapX = MathF.Min(aBounds.Right, bBounds.Right) - MathF.Max(aBounds.Left, bBounds.Left);
        float overlapY = MathF.Min(aBounds.Bottom, bBounds.Bottom) - MathF.Max(aBounds.Top, bBounds.Top);
        var (cax, cay) = rectA.WorldCenter(ax, ay);
        var (cbx, cby) = rectB.WorldCenter(bx, by);

        if (overlapX < overlapY)
        {
            hit = new CollisionHit(cax < cbx ? -1f : 1f, 0f, overlapX);
            return true;
        }

        hit = new CollisionHit(0f, cay < cby ? -1f : 1f, overlapY);
        return true;
    }

    private static bool TryGetCircleRectHit(float circleX, float circleY, CircleCollider circle,
                                             float rectX, float rectY, RectCollider rect,
                                             out CollisionHit hit)
    {
        var (cx, cy) = circle.WorldCenter(circleX, circleY);
        var bounds = rect.WorldRect(rectX, rectY);
        float nearX = Math.Clamp(cx, bounds.Left, bounds.Right);
        float nearY = Math.Clamp(cy, bounds.Top, bounds.Bottom);
        float dx = cx - nearX;
        float dy = cy - nearY;
        float distanceSquared = dx * dx + dy * dy;
        float radiusSquared = circle.Radius * circle.Radius;

        if (distanceSquared >= radiusSquared)
        {
            hit = default;
            return false;
        }

        if (distanceSquared > 0f)
        {
            float distance = MathF.Sqrt(distanceSquared);
            hit = new CollisionHit(dx / distance, dy / distance, circle.Radius - distance);
            return true;
        }

        // The circle center is inside the rectangle. Push it out on the shallowest axis.
        float left = cx - bounds.Left;
        float right = bounds.Right - cx;
        float top = cy - bounds.Top;
        float bottom = bounds.Bottom - cy;
        float minHorizontal = MathF.Min(left, right);
        float minVertical = MathF.Min(top, bottom);

        if (minHorizontal < minVertical)
        {
            hit = new CollisionHit(left < right ? -1f : 1f, 0f, circle.Radius + minHorizontal);
            return true;
        }

        hit = new CollisionHit(0f, top < bottom ? -1f : 1f, circle.Radius + minVertical);
        return true;
    }
}
