using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws the lord with crown, HP bar, and sword.</summary>
internal sealed class LordSprite : Sprite
{
    public float HPRatio;
    public bool Active;

    private static readonly SKPaint BodyPaint = new() { Color = ColLord, IsAntialias = true };
    private static readonly SKPaint CrownPaint = new() { Color = ColGold, IsAntialias = true };
    private static readonly SKPaint SwordPaint = new() { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 2f, IsAntialias = true };
    private static readonly SKPaint HpBarBg = new() { Color = new SKColor(0x40, 0x00, 0x00) };
    private static readonly SKPaint HpBarFg = new() { Color = new SKColor(0xFF, 0x22, 0x22) };

    /// <param name="x">Centre X of the lord.</param>
    /// <param name="y">Base Y (feet) of the lord.</param>
    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Active || !Visible) return;

        const float barW = 40f;
        canvas.DrawRect(SKRect.Create(x - barW / 2f, y - LordH - 14f, barW, 6f), HpBarBg);
        HpBarFg.Color = new SKColor(0xFF, 0x22, 0x22);
        canvas.DrawRect(SKRect.Create(x - barW / 2f, y - LordH - 14f, barW * HPRatio, 6f), HpBarFg);

        canvas.DrawRect(SKRect.Create(x - LordW / 2f, y - LordH, LordW, LordH - 8f), BodyPaint);
        canvas.DrawCircle(x, y - LordH - 5f, 7f, BodyPaint);

        // Crown
        canvas.DrawRect(SKRect.Create(x - 7f, y - LordH - 17f, 4f, 6f), CrownPaint);
        canvas.DrawRect(SKRect.Create(x - 2f, y - LordH - 20f, 4f, 9f), CrownPaint);
        canvas.DrawRect(SKRect.Create(x + 3f, y - LordH - 17f, 4f, 6f), CrownPaint);

        // Sword
        canvas.DrawLine(x + LordW / 2f, y - LordH, x + LordW / 2f + 18f, y - LordH / 2f, SwordPaint);
    }
}
