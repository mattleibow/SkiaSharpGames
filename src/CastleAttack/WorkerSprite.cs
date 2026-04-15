using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws a worker figure (body rect, head circle, tool line).</summary>
internal sealed class WorkerSprite : Sprite
{
    private static readonly SKPaint BodyPaint = new() { Color = ColWorker, IsAntialias = true };
    private static readonly SKPaint ToolPaint = new() { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 2f, IsAntialias = true };

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible) return;

        canvas.DrawRect(SKRect.Create(0f - WorkerW / 2f, 0f - WorkerH, WorkerW, WorkerH - 6f), BodyPaint);
        canvas.DrawCircle(0f, 0f - WorkerH - 4f, 5f, BodyPaint);
        canvas.DrawLine(0f + WorkerW / 2f, 0f - WorkerH + 2f, 0f + WorkerW / 2f + 10f, 0f - WorkerH + 10f, ToolPaint);
    }
}
