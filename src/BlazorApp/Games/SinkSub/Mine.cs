using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.BlazorApp.Games.SinkSub;

internal sealed class Mine : Entity
{
    public readonly CircleCollider Collider = new() { Radius = MineRadius };
    public readonly Rigidbody2D Rigidbody = new();
    public readonly CircleSprite Sprite = new()
    {
        Radius = MineRadius,
        Color = new SKColor(0xD9, 0x78, 0x24),
        GlowRadius = 4f,
        GlowColor = new SKColor(0xFF, 0xBF, 0x66),
    };
}
