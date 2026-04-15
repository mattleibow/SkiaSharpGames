using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws a boulder circle.</summary>
internal sealed class BoulderSprite : Sprite
{
    private static readonly SKPaint Paint = new() { Color = ColBoulder, IsAntialias = true };

    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible) return;
        canvas.DrawCircle(x, y, 7f, Paint);
    }
}
