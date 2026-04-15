using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

internal sealed class PowerUpSprite : Sprite
{
    private readonly SKPaint _paint = new() { IsAntialias = true };

    public float Width { get; set; } = PowerUpW;

    public float Height { get; set; } = PowerUpH;

    public SKColor Color { get; set; } = SKColors.White;

    public float CornerRadius { get; set; } = 5f;

    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible || Alpha <= 0f)
            return;

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        canvas.DrawRoundRect(
            new SKRoundRect(SKRect.Create(x - Width / 2f, y - Height / 2f, Width, Height), CornerRadius),
            _paint);
    }
}
