using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class DepthChargeSprite : Sprite
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPaint _glowPaint = new() { IsAntialias = true };
    private float _cachedGlowRadius = float.NaN;

    public float Radius { get; set; } = ChargeRadius;

    public SKColor Color { get; set; } = new SKColor(0x20, 0x20, 0x28);

    public float GlowRadius { get; set; } = 2f;

    public SKColor GlowColor { get; set; } = SKColors.White;

    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible || Alpha <= 0f)
            return;

        if (GlowRadius > 0f)
        {
            EnsureGlowMask();
            _glowPaint.Color = GlowColor.WithAlpha((byte)(45 * Alpha));
            canvas.DrawCircle(x, y, Radius + GlowRadius / 2f, _glowPaint);
        }

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        canvas.DrawCircle(x, y, Radius, _paint);
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
