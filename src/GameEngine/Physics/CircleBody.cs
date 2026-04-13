using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A circular physics body whose position (<see cref="PhysicsBody.X"/>, <see cref="PhysicsBody.Y"/>)
/// is the centre of the circle.
/// </summary>
public sealed class CircleBody : PhysicsBody
{
    /// <summary>Radius of the circle in game-space units.</summary>
    public float Radius { get; set; }

    /// <inheritdoc/>
    public override SKRect BoundingBox =>
        new SKRect(X - Radius, Y - Radius, X + Radius, Y + Radius);
}
