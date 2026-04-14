using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// An axis-aligned rectangular collision shape attached to an <see cref="Entity"/>.
/// The entity's <see cref="Entity.X"/>/<see cref="Entity.Y"/> is the centre of the rectangle.
/// <see cref="OffsetX"/>/<see cref="OffsetY"/> shift the hitbox relative to that centre.
/// </summary>
public sealed class RectCollider
{
    /// <summary>Width of the rectangle in game-space units.</summary>
    public float Width { get; set; }

    /// <summary>Height of the rectangle in game-space units.</summary>
    public float Height { get; set; }

    /// <summary>Horizontal offset of the hitbox centre relative to the owning entity's position.</summary>
    public float OffsetX { get; set; }

    /// <summary>Vertical offset of the hitbox centre relative to the owning entity's position.</summary>
    public float OffsetY { get; set; }

    /// <summary>World-space axis-aligned bounding box for the given <paramref name="owner"/>.</summary>
    public SKRect WorldRect(Entity owner) => SKRect.Create(
        owner.X + OffsetX - Width / 2f,
        owner.Y + OffsetY - Height / 2f,
        Width, Height);
}
