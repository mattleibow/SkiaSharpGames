using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.Breakout.BreakoutConstants;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

internal sealed class FallingPowerUp : Entity
{
    public PowerUpType Type;
    public readonly Rigidbody2D Rigidbody = new() { VelocityY = PowerUpSpeed };
    public readonly RectCollider Collider = new()
    {
        Width = PowerUpW,
        Height = PowerUpH,
    };
    public readonly RectSprite Sprite = new()
    {
        Width = PowerUpW,
        Height = PowerUpH,
        CornerRadius = 5f,
        ShowShine = false,
    };
}
