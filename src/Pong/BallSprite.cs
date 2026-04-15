using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.Pong;

internal sealed class BallSprite : Sprite
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPaint _glowPaint = new() { IsAntialias = true, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2f) };

    public float Radius { get; set; } = 10f;
    public SKColor Color { get; set; } = SKColors.White;
    public SKColor GlowColor { get; set; } = SKColors.White;
    public float GlowRadius { get; set; } = 2f;

    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible || Alpha <= 0f)
            return;

        byte a = (byte)(255 * Alpha);

        if (GlowRadius > 0f)
        {
            _glowPaint.Color = GlowColor.WithAlpha((byte)(a * 0.5f));
            canvas.DrawCircle(x, y, Radius + GlowRadius, _glowPaint);
        }

        _paint.Color = Color.WithAlpha(a);
        canvas.DrawCircle(x, y, Radius, _paint);
    }
}
