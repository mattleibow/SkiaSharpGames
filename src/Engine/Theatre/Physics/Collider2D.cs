namespace SkiaSharp.Theatre;

/// <summary>
/// Base type for simple 2D colliders attached to an <see cref="Actor"/>.
/// The collider describes the hit shape around a centre point; the actor's
/// world position is passed to <see cref="BoundingBox"/> and used by
/// <see cref="CollisionResolver"/>.
/// </summary>
public abstract class Collider2D
{
    /// <summary>
    /// Horizontal offset of the collider relative to the owning actor center.
    /// Offset is applied in world space and does not rotate with the actor.
    /// For rotated actors, keep offsets at zero.
    /// </summary>
    public float OffsetX { get; set; }

    /// <summary>
    /// Vertical offset of the collider relative to the owning actor center.
    /// Offset is applied in world space and does not rotate with the actor.
    /// For rotated actors, keep offsets at zero.
    /// </summary>
    public float OffsetY { get; set; }

    /// <summary>Gets the collider center given the actor's world position.</summary>
    public (float X, float Y) WorldCenter(float actorX, float actorY) =>
        (actorX + OffsetX, actorY + OffsetY);

    /// <summary>Gets the world-space axis-aligned bounds of the collider.</summary>
    public abstract SKRect BoundingBox(float centerX, float centerY);
}
