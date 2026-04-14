using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.Breakout.BreakoutConstants;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

/// <summary>
/// The player's ball entity. Position (X, Y) is the centre of the circle.
/// </summary>
internal sealed class Ball : Entity
{
    public readonly CircleCollider Collider = new() { Radius = BallRadius };
    public readonly Rigidbody2D Rigidbody = new();
    public readonly CircleSprite Sprite = new()
    {
        Radius = BallRadius,
        Color = SKColors.White,
        GlowRadius = 4f,
        GlowColor = SKColors.White,
    };
}
