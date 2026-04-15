using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// The player's ball entity. Position (X, Y) is the centre of the circle.
/// </summary>
internal sealed class Ball : Entity
{
    public readonly CircleCollider Collider = new() { Radius = BallRadius };
    public readonly Rigidbody2D Rigidbody = new();
    public readonly BallSprite Sprite = new();
}
