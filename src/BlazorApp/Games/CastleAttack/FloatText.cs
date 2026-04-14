using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

internal sealed class FloatText : Entity
{
    public readonly Rigidbody2D Rigidbody = new() { VelocityY = -28f };
    public float Life;
    public string Text = "";
    public SKColor Color = SKColors.White;
}
