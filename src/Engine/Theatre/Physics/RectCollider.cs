using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// An axis-aligned rectangular collision shape attached to an <see cref="Actor"/>.
/// </summary>
public sealed class RectCollider : Collider2D
{
    /// <summary>Width of the rectangle in game-space units.</summary>
    public float Width { get; set; }

    /// <summary>Height of the rectangle in game-space units.</summary>
    public float Height { get; set; }

    /// <summary>World-space rectangle given an actor centre position.</summary>
    public SKRect WorldRect(float centerX, float centerY) => SKRect.Create(
        centerX + OffsetX - Width / 2f,
        centerY + OffsetY - Height / 2f,
        Width, Height);

    /// <inheritdoc/>
    public override SKRect BoundingBox(float centerX, float centerY) =>
        WorldRect(centerX, centerY);
}
