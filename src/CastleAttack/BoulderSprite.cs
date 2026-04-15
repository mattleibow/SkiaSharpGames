using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws a boulder circle.</summary>
internal sealed class BoulderSprite : Sprite
{
    private static readonly SKPaint Paint = new() { Color = ColBoulder, IsAntialias = true };

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible) return;
        canvas.DrawCircle(0f, 0f, 7f, Paint);
    }
}
