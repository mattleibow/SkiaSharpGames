using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.BlazorApp.Games.SinkSub;

internal sealed class DepthCharge : Entity
{
    public readonly CircleCollider Collider = new() { Radius = ChargeRadius };
    public readonly Rigidbody2D Rigidbody = new();
    public readonly CircleSprite Sprite = new()
    {
        Radius = ChargeRadius,
        Color = new SKColor(0x20, 0x20, 0x28),
        GlowRadius = 2f,
        GlowColor = SKColors.White,
    };
}
