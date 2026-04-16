using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws an archer figure (body rect, head circle, bow line).</summary>
internal sealed class ArcherSprite : Sprite
{
    private static readonly SKPaint BodyPaint = new() { Color = ColArcher, IsAntialias = true };
    private static readonly SKPaint BowPaint = new() { Color = ColArrow, StrokeWidth = 2f, IsAntialias = true };

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible) return;

        canvas.DrawRect(SKRect.Create(0f - ArcherW / 2f, 0f - ArcherH, ArcherW, ArcherH - 8f), BodyPaint);
        canvas.DrawCircle(0f, 0f - ArcherH - 5f, 6f, BodyPaint);
        canvas.DrawLine(0f + ArcherW / 2f, 0f - ArcherH + 4f, 0f + ArcherW / 2f + 8f, 0f - ArcherH / 2f, BowPaint);
    }
}
