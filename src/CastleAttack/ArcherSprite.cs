using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws an archer figure (body rect, head circle, bow line).</summary>
internal sealed class ArcherSprite : Sprite
{
    private static readonly SKPaint BodyPaint = new() { Color = ColArcher, IsAntialias = true };
    private static readonly SKPaint BowPaint = new() { Color = ColArrow, StrokeWidth = 2f, IsAntialias = true };

    /// <param name="x">Centre X of the archer.</param>
    /// <param name="y">Base Y (feet) of the archer.</param>
    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible) return;

        canvas.DrawRect(SKRect.Create(x - ArcherW / 2f, y - ArcherH, ArcherW, ArcherH - 8f), BodyPaint);
        canvas.DrawCircle(x, y - ArcherH - 5f, 6f, BodyPaint);
        canvas.DrawLine(x + ArcherW / 2f, y - ArcherH + 4f, x + ArcherW / 2f + 8f, y - ArcherH / 2f, BowPaint);
    }
}
