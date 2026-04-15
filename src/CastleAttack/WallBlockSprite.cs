using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws a single wall block with damage colouring and crack effects.</summary>
internal sealed class WallBlockSprite : Sprite
{
    public float HPRatio;
    public bool Flash;

    private static readonly SKPaint BlockPaint = new() { IsAntialias = true };
    private static readonly SKPaint CrackPaint = new() { Color = new SKColor(0x00, 0x00, 0x00, 80), StrokeWidth = 1.5f, IsAntialias = true };
    private static readonly SKPaint ShinePaint = new() { Color = SKColors.White.WithAlpha(40), IsAntialias = true };

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible) return;

        SKColor col = Flash ? ColStoneDmg
                    : HPRatio < 0.3f ? ColStoneLow
                    : HPRatio < 0.6f ? ColStoneDmg
                    : ColStone;

        BlockPaint.Color = col;
        canvas.DrawRoundRect(SKRect.Create(0f, 0f, BlockW, BlockH), 3f, 3f, BlockPaint);

        if (HPRatio < 0.6f)
            canvas.DrawLine(0f + 8f, 0f + 4f, 0f + 18f, 0f + BlockH - 4f, CrackPaint);

        canvas.DrawRect(SKRect.Create(0f + 2f, 0f + 2f, BlockW - 4f, BlockH / 2f - 2f), ShinePaint);
    }
}
