using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws a burning oil puddle on the ground — a flat orange rectangle that flickers.</summary>
internal sealed class OilPuddleSprite : Sprite
{
    public float Life;

    private static readonly SKColor CoreColor = new(0xFF, 0x6B, 0x00);
    private static readonly SKColor GlowColor = new(0xFF, 0x44, 0x00, 80);

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible) return;

        byte alpha = (byte)(255 * Math.Clamp(Life / OilPuddleDuration, 0f, 1f));
        using var corePaint = new SKPaint { Color = CoreColor.WithAlpha(alpha), IsAntialias = true };
        using var glowPaint = new SKPaint { Color = GlowColor.WithAlpha((byte)(alpha / 3)), IsAntialias = true };

        float hw = OilPuddleWidth / 2f;
        float hh = OilPuddleHeight / 2f;

        // Glow underneath
        canvas.DrawRoundRect(-hw - 4f, -hh - 2f, OilPuddleWidth + 8f, OilPuddleHeight + 4f, 4f, 2f, glowPaint);
        // Core puddle
        canvas.DrawRoundRect(-hw, -hh, OilPuddleWidth, OilPuddleHeight, 3f, 2f, corePaint);
    }
}
