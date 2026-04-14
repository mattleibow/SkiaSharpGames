using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// An axis-aligned rectangular physics body whose position (<see cref="PhysicsBody.X"/>,
/// <see cref="PhysicsBody.Y"/>) is the top-left corner.
/// </summary>
public sealed class RectBody : PhysicsBody
{
    /// <summary>Width of the rectangle in game-space units.</summary>
    public float Width { get; set; }

    /// <summary>Height of the rectangle in game-space units.</summary>
    public float Height { get; set; }

    /// <inheritdoc/>
    public override SKRect BoundingBox =>
        new SKRect(X, Y, X + Width, Y + Height);
}
