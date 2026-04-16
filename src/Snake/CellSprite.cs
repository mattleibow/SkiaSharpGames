using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

/// <summary>Draws a single rounded-rect cell, centred at the entity origin.</summary>
internal sealed class CellSprite : Sprite
{
    private static readonly SKPaint _paint = new() { IsAntialias = true };

    public SKColor Color { get; set; } = SKColors.White;

    public float Inset { get; set; } = 2f;

    public float CornerRadius { get; set; } = 6f;

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible || Alpha <= 0f)
            return;

        float size = CellSize - Inset * 2f;
        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        canvas.DrawRoundRect(-size / 2f, -size / 2f, size, size, CornerRadius, CornerRadius, _paint);
    }
}
