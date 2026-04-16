using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws a falling oil drop as a dark brown circle with a faint glow.</summary>
internal sealed class OilDropSprite : Sprite
{
    private static readonly SKPaint CorePaint = new() { Color = ColOil, IsAntialias = true };
    private static readonly SKPaint GlowPaint = new() { Color = new SKColor(0xFF, 0x6B, 0x00, 60), IsAntialias = true };

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible) return;
        canvas.DrawCircle(0f, 0f, OilDropRadius + 3f, GlowPaint);
        canvas.DrawCircle(0f, 0f, OilDropRadius, CorePaint);
    }
}
