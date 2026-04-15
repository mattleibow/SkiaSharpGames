using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class PongBall : Entity
{
    public readonly CircleCollider Collider = new() { Radius = BallRadius };
    public readonly Rigidbody2D Rigidbody = new();
    public readonly BallSprite Sprite = new()
    {
        Radius = BallRadius,
        Color = SKColors.White,
        GlowRadius = 2f,
        GlowColor = SKColors.White,
    };
}
