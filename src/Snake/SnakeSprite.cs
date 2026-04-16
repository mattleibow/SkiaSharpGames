using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

/// <summary>Draws the full snake body as a chain of rounded-rect segments.</summary>
internal sealed class SnakeSprite : Sprite
{
    private static readonly SKPaint _paint = new() { IsAntialias = true };

    /// <summary>The body segments to draw (head first).</summary>
    public LinkedList<GridPoint>? Body { get; set; }

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible || Alpha <= 0f || Body is null)
            return;

        bool isHead = true;
        foreach (var seg in Body)
        {
            _paint.Color = (isHead ? SnakeHeadColor : SnakeBodyColor).WithAlpha((byte)(255 * Alpha));
            float cx = seg.Col * CellSize + CellSize / 2f;
            float cy = seg.Row * CellSize + CellSize / 2f;
            canvas.DrawRoundRect(cx - CellSize / 2f + 1f, cy - CellSize / 2f + 1f,
                                 CellSize - 2f, CellSize - 2f, 4f, 4f, _paint);
            isHead = false;
        }
    }
}
