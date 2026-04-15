using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

internal sealed class FallingPowerUp : Entity
{
    public PowerUpType Type;
    public readonly Rigidbody2D Rigidbody = new() { VelocityY = PowerUpSpeed };
    public readonly RectCollider Collider = new()
    {
        Width = PowerUpW,
        Height = PowerUpH,
    };
    public readonly PowerUpSprite Sprite = new()
    {
        Width = PowerUpW,
        Height = PowerUpH,
    };
}
