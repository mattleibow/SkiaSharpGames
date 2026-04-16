using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

internal sealed class BallSprite : Sprite
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPaint _glowPaint = new() { IsAntialias = true };
    private float _cachedGlowRadius = float.NaN;

    public float Radius { get; set; } = BallRadius;

    public SKColor Color { get; set; } = SKColors.White;

    public float GlowRadius { get; set; } = 4f;

    public SKColor GlowColor { get; set; } = SKColors.White;

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible || Alpha <= 0f)
            return;

        if (GlowRadius > 0f)
        {
            EnsureGlowMask();
            _glowPaint.Color = GlowColor.WithAlpha((byte)(60 * Alpha));
            canvas.DrawCircle(0, 0, Radius + GlowRadius / 2f, _glowPaint);
        }

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        canvas.DrawCircle(0, 0, Radius, _paint);
    }

    private void EnsureGlowMask()
    {
        if (GlowRadius.Equals(_cachedGlowRadius))
            return;

        _glowPaint.MaskFilter?.Dispose();
        _glowPaint.MaskFilter = GlowRadius > 0f
            ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowRadius)
            : null;
        _cachedGlowRadius = GlowRadius;
    }
}
