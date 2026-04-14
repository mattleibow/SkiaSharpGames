namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Static helpers for simple arcade-style 2D collisions between entities, colliders, and
/// rigidbodies.
/// </summary>
public static class CollisionResolver
{
    // ── Wall / bounds resolution ──────────────────────────────────────────

    /// <summary>
    /// Resolves left-wall, right-wall, and ceiling collisions for a circle moving through a bounded
    /// game area. The bottom boundary is intentionally excluded so callers can decide whether that
    /// means "lose a life", "wrap around", or "bounce".
    /// </summary>
    public static void ResolveWalls(Entity entity, CircleCollider collider, Rigidbody2D rigidbody, float gameWidth)
        => ResolveBounds(entity, collider, rigidbody, new GameBounds(0f, 0f, gameWidth, float.MaxValue), bounceBottom: false);

    /// <summary>
    /// Keeps a circle inside the supplied playfield bounds. When an enabled boundary is crossed,
    /// the position is corrected and the velocity is reflected.
    /// </summary>
    public static bool ResolveBounds(Entity entity,
                                     CircleCollider collider,
                                     Rigidbody2D rigidbody,
                                     in GameBounds bounds,
                                     bool bounceLeft = true,
                                     bool bounceTop = true,
                                     bool bounceRight = true,
                                     bool bounceBottom = true)
    {
        bool hit = false;
        float r = collider.Radius;

        if (entity.X - r < bounds.Left)
        {
            entity.X = bounds.Left + r;
            if (bounceLeft && rigidbody.VelocityX < 0f)
                rigidbody.BounceX();
            hit = true;
        }
        else if (entity.X + r > bounds.Right)
        {
            entity.X = bounds.Right - r;
            if (bounceRight && rigidbody.VelocityX > 0f)
                rigidbody.BounceX();
            hit = true;
        }

        if (entity.Y - r < bounds.Top)
        {
            entity.Y = bounds.Top + r;
            if (bounceTop && rigidbody.VelocityY < 0f)
                rigidbody.BounceY();
            hit = true;
        }
        else if (entity.Y + r > bounds.Bottom)
        {
            entity.Y = bounds.Bottom - r;
            if (bounceBottom && rigidbody.VelocityY > 0f)
                rigidbody.BounceY();
            hit = true;
        }

        return hit;
    }

    // ── Entity + collider API ─────────────────────────────────────────────

    /// <summary>
    /// Returns <see langword="true"/> when the two colliders overlap.
    /// </summary>
    public static bool Overlaps(Entity a, Collider2D colliderA, Entity b, Collider2D colliderB) =>
        TryGetHit(a, colliderA, b, colliderB, out _);

    /// <summary>
    /// Returns <see langword="true"/> when the circle described by
    /// <paramref name="circle"/>/<paramref name="circleOwner"/> overlaps the rectangle described by
    /// <paramref name="rect"/>/<paramref name="rectOwner"/>.
    /// </summary>
    public static bool Overlaps(Entity circleOwner, CircleCollider circle,
                                Entity rectOwner,   RectCollider rect)
        => TryGetHit(circleOwner, circle, rectOwner, rect, out _);

    /// <summary>
    /// Tries to compute collision information for the two supplied colliders.
    /// </summary>
    public static bool TryGetHit(Entity a, Collider2D colliderA, Entity b, Collider2D colliderB, out CollisionHit hit)
    {
        switch (colliderA)
        {
            case CircleCollider circleA when colliderB is CircleCollider circleB:
                return TryGetCircleCircleHit(a, circleA, b, circleB, out hit);

            case CircleCollider circle when colliderB is RectCollider rect:
                return TryGetCircleRectHit(a, circle, b, rect, out hit);

            case RectCollider rectA when colliderB is CircleCollider circleB:
                if (TryGetCircleRectHit(b, circleB, a, rectA, out var reverseHit))
                {
                    hit = new CollisionHit(-reverseHit.NormalX, -reverseHit.NormalY, reverseHit.Penetration);
                    return true;
                }
                hit = default;
                return false;

            case RectCollider rectA when colliderB is RectCollider rectB:
                return TryGetRectRectHit(a, rectA, b, rectB, out hit);

            default:
                throw new NotSupportedException($"Unsupported collider pair: {colliderA.GetType().Name} and {colliderB.GetType().Name}.");
        }
    }

    /// <summary>
    /// Reflects <paramref name="rigidbody"/>'s velocity off the rectangle
    /// <paramref name="rect"/>/<paramref name="rectOwner"/> using the smallest-overlap axis.
    /// Does <b>not</b> check for overlap first.
    /// </summary>
    public static void Reflect(Entity circleOwner, CircleCollider circle, Rigidbody2D rigidbody,
                               Entity rectOwner,   RectCollider rect)
    {
        if (TryGetHit(circleOwner, circle, rectOwner, rect, out var hit))
            rigidbody.Bounce(hit);
    }

    /// <summary>
    /// If the circle overlaps the rectangle, reflects <paramref name="rigidbody"/>'s velocity
    /// and returns <see langword="true"/>. Returns <see langword="false"/> when there is no overlap.
    /// </summary>
    public static bool ReflectOff(Entity circleOwner, CircleCollider circle, Rigidbody2D rigidbody,
                                  Entity rectOwner,   RectCollider rect)
    {
        if (!TryGetHit(circleOwner, circle, rectOwner, rect, out var hit))
            return false;

        rigidbody.Bounce(hit);
        return true;
    }

    private static bool TryGetCircleCircleHit(Entity a, CircleCollider circleA,
                                              Entity b, CircleCollider circleB,
                                              out CollisionHit hit)
    {
        var (ax, ay) = circleA.WorldCenter(a);
        var (bx, by) = circleB.WorldCenter(b);
        float dx = ax - bx;
        float dy = ay - by;
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

    private static bool TryGetRectRectHit(Entity a, RectCollider rectA,
                                          Entity b, RectCollider rectB,
                                          out CollisionHit hit)
    {
        var aBounds = rectA.WorldRect(a);
        var bBounds = rectB.WorldRect(b);

        if (!aBounds.IntersectsWith(bBounds))
        {
            hit = default;
            return false;
        }

        float overlapX = MathF.Min(aBounds.Right, bBounds.Right) - MathF.Max(aBounds.Left, bBounds.Left);
        float overlapY = MathF.Min(aBounds.Bottom, bBounds.Bottom) - MathF.Max(aBounds.Top, bBounds.Top);
        var (ax, ay) = rectA.WorldCenter(a);
        var (bx, by) = rectB.WorldCenter(b);

        if (overlapX < overlapY)
        {
            hit = new CollisionHit(ax < bx ? -1f : 1f, 0f, overlapX);
            return true;
        }

        hit = new CollisionHit(0f, ay < by ? -1f : 1f, overlapY);
        return true;
    }

    private static bool TryGetCircleRectHit(Entity circleOwner, CircleCollider circle,
                                            Entity rectOwner, RectCollider rect,
                                            out CollisionHit hit)
    {
        var (cx, cy) = circle.WorldCenter(circleOwner);
        var bounds = rect.WorldRect(rectOwner);
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
