using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Pong;

internal sealed class PaddleSprite : Sprite
{
    private readonly SKPaint _paint = new() { IsAntialias = true };

    public float Width { get; set; }
    public float Height { get; set; }
    public SKColor Color { get; set; }
    public float CornerRadius { get; set; } = 5f;

    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible || Alpha <= 0f)
            return;

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        var rect = SKRect.Create(x - Width / 2f, y - Height / 2f, Width, Height);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _paint);
    }
}
