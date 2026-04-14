using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// An axis-aligned rectangular collision shape attached to an <see cref="Entity"/>.
/// The entity's <see cref="Entity.X"/>/<see cref="Entity.Y"/> is the centre of the rectangle.
/// <see cref="Collider2D.OffsetX"/>/<see cref="Collider2D.OffsetY"/> shift the hitbox relative
/// to that centre.
/// </summary>
public sealed class RectCollider : Collider2D
{
    /// <summary>Width of the rectangle in game-space units.</summary>
    public float Width { get; set; }

    /// <summary>Height of the rectangle in game-space units.</summary>
    public float Height { get; set; }

    /// <summary>World-space axis-aligned bounding box for the given <paramref name="owner"/>.</summary>
    public SKRect WorldRect(Entity owner) => SKRect.Create(
        owner.X + OffsetX - Width / 2f,
        owner.Y + OffsetY - Height / 2f,
        Width, Height);

    /// <summary>World-space axis-aligned bounding box for the given <paramref name="owner"/>.</summary>
    public override SKRect BoundingBox(Entity owner) => WorldRect(owner);
}
