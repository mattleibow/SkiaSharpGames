namespace SkiaSharp.Theatre;

/// <summary>
/// A circular collision shape attached to an <see cref="Actor"/>.
/// </summary>
public sealed class CircleCollider : Collider2D
{
    /// <summary>Radius of the circle in game-space units.</summary>
    public float Radius { get; set; }

    /// <inheritdoc/>
    public override SKRect BoundingBox(float centerX, float centerY)
    {
        float cx = centerX + OffsetX;
        float cy = centerY + OffsetY;
        return new SKRect(cx - Radius, cy - Radius, cx + Radius, cy + Radius);
    }
}