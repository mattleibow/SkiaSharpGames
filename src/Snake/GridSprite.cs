using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

/// <summary>Draws the background grid lines.</summary>
internal sealed class GridSprite : Sprite
{
    private static readonly SKPaint _linePaint = new() { Color = GridLineColor, StrokeWidth = 1f, IsAntialias = true };

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible || Alpha <= 0f)
            return;

        for (int c = 1; c < GridCols; c++)
        {
            float x = c * CellSize;
            canvas.DrawLine(x, 0, x, GameHeight, _linePaint);
        }

        for (int r = 1; r < GridRows; r++)
        {
            float y = r * CellSize;
            canvas.DrawLine(0, y, GameWidth, y, _linePaint);
        }
    }
}
