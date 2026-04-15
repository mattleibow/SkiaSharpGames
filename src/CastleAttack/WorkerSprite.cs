using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws a worker figure (body rect, head circle, tool line).</summary>
internal sealed class WorkerSprite : Sprite
{
    private static readonly SKPaint BodyPaint = new() { Color = ColWorker, IsAntialias = true };
    private static readonly SKPaint ToolPaint = new() { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 2f, IsAntialias = true };

    /// <param name="x">Centre X of the worker.</param>
    /// <param name="y">Base Y (feet) of the worker.</param>
    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible) return;

        canvas.DrawRect(SKRect.Create(x - WorkerW / 2f, y - WorkerH, WorkerW, WorkerH - 6f), BodyPaint);
        canvas.DrawCircle(x, y - WorkerH - 4f, 5f, BodyPaint);
        canvas.DrawLine(x + WorkerW / 2f, y - WorkerH + 2f, x + WorkerW / 2f + 10f, y - WorkerH + 10f, ToolPaint);
    }
}
