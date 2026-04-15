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

    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible) return;

        SKColor col = Flash ? ColStoneDmg
                    : HPRatio < 0.3f ? ColStoneLow
                    : HPRatio < 0.6f ? ColStoneDmg
                    : ColStone;

        BlockPaint.Color = col;
        canvas.DrawRoundRect(SKRect.Create(x, y, BlockW, BlockH), 3f, 3f, BlockPaint);

        if (HPRatio < 0.6f)
            canvas.DrawLine(x + 8f, y + 4f, x + 18f, y + BlockH - 4f, CrackPaint);

        canvas.DrawRect(SKRect.Create(x + 2f, y + 2f, BlockW - 4f, BlockH / 2f - 2f), ShinePaint);
    }
}
