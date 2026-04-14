using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A circular collision shape attached to an <see cref="Entity"/>.
/// The entity's <see cref="Entity.X"/>/<see cref="Entity.Y"/> is the centre of the circle.
/// <see cref="OffsetX"/>/<see cref="OffsetY"/> shift the hitbox relative to that centre.
/// </summary>
public sealed class CircleCollider
{
    /// <summary>Radius of the circle in game-space units.</summary>
    public float Radius { get; set; }

    /// <summary>Horizontal offset of the hitbox centre relative to the owning entity's position.</summary>
    public float OffsetX { get; set; }

    /// <summary>Vertical offset of the hitbox centre relative to the owning entity's position.</summary>
    public float OffsetY { get; set; }

    /// <summary>World-space centre of the circle for the given <paramref name="owner"/>.</summary>
    public (float X, float Y) WorldCenter(Entity owner) =>
        (owner.X + OffsetX, owner.Y + OffsetY);

    /// <summary>World-space axis-aligned bounding box for the given <paramref name="owner"/>.</summary>
    public SKRect BoundingBox(Entity owner)
    {
        var (cx, cy) = WorldCenter(owner);
        return new SKRect(cx - Radius, cy - Radius, cx + Radius, cy + Radius);
    }
}
