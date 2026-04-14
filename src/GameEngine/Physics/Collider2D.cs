using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Base type for simple 2D colliders attached to an <see cref="Entity"/>.
/// The entity remains the single source of truth for world position; the collider only
/// describes the hit shape around that position.
/// </summary>
public abstract class Collider2D
{
    /// <summary>Horizontal offset of the collider relative to the owning entity center.</summary>
    public float OffsetX { get; set; }

    /// <summary>Vertical offset of the collider relative to the owning entity center.</summary>
    public float OffsetY { get; set; }

    /// <summary>Gets the collider center in world space.</summary>
    public virtual (float X, float Y) WorldCenter(Entity owner) =>
        (owner.X + OffsetX, owner.Y + OffsetY);

    /// <summary>Gets the world-space axis-aligned bounds of the collider.</summary>
    public abstract SKRect BoundingBox(Entity owner);
}
