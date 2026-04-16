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
    public override void Draw(SKCanvas canvas)
    {
        if (!Active || !Visible) return;

        const float barW = 40f;
        canvas.DrawRect(SKRect.Create(0f - barW / 2f, 0f - LordH - 14f, barW, 6f), HpBarBg);
        HpBarFg.Color = new SKColor(0xFF, 0x22, 0x22);
        canvas.DrawRect(SKRect.Create(0f - barW / 2f, 0f - LordH - 14f, barW * HPRatio, 6f), HpBarFg);

        canvas.DrawRect(SKRect.Create(0f - LordW / 2f, 0f - LordH, LordW, LordH - 8f), BodyPaint);
        canvas.DrawCircle(0f, 0f - LordH - 5f, 7f, BodyPaint);

        // Crown
        canvas.DrawRect(SKRect.Create(0f - 7f, 0f - LordH - 17f, 4f, 6f), CrownPaint);
        canvas.DrawRect(SKRect.Create(0f - 2f, 0f - LordH - 20f, 4f, 9f), CrownPaint);
        canvas.DrawRect(SKRect.Create(0f + 3f, 0f - LordH - 17f, 4f, 6f), CrownPaint);

        // Sword
        canvas.DrawLine(0f + LordW / 2f, 0f - LordH, 0f + LordW / 2f + 18f, 0f - LordH / 2f, SwordPaint);
    }
}
