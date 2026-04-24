namespace SkiaSharp.Theatre;

/// <summary>
/// An axis-aligned rectangular collision shape attached to an <see cref="Actor"/>.
/// </summary>
public sealed class RectCollider(float width, float height) : Collider2D
{
    /// <summary>
    /// Creates a new <see cref="RectCollider"/> with the no width or height.
    /// </summary>
    public RectCollider()
        : this(width: 0f, height: 0f) { }

    /// <summary>Width of the rectangle in game-space units.</summary>
    public float Width { get; set; } = width;

    /// <summary>Height of the rectangle in game-space units.</summary>
    public float Height { get; set; } = height;

    /// <summary>World-space rectangle given an actor centre position.</summary>
    public SKRect WorldRect(float centerX, float centerY) =>
        SKRect.Create(
            centerX + OffsetX - Width / 2f,
            centerY + OffsetY - Height / 2f,
            Width,
            Height
        );

    /// <inheritdoc/>
    public override SKRect BoundingBox(float centerX, float centerY) => WorldRect(centerX, centerY);
}
