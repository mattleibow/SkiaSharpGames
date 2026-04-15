using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

internal sealed class FloatText : Entity
{
    public readonly Rigidbody2D Rigidbody = new() { VelocityY = -28f };
    public float Life;
    public string Text = "";
    public SKColor Color = SKColors.White;

    public readonly FloatTextSprite Sprite = new();
}
