using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A circular collision shape attached to an <see cref="Entity"/>.
/// The entity's <see cref="Entity.X"/>/<see cref="Entity.Y"/> is the centre of the circle.
/// <see cref="Collider2D.OffsetX"/>/<see cref="Collider2D.OffsetY"/> shift the hitbox relative
/// to that centre.
/// </summary>
public sealed class CircleCollider : Collider2D
{
    /// <summary>Radius of the circle in game-space units.</summary>
    public float Radius { get; set; }

    /// <summary>World-space axis-aligned bounding box for the given <paramref name="owner"/>.</summary>
    public override SKRect BoundingBox(Entity owner)
    {
        var (cx, cy) = WorldCenter(owner);
        return new SKRect(cx - Radius, cy - Radius, cx + Radius, cy + Radius);
    }
}
