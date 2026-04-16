using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class MineSprite : Sprite
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPaint _glowPaint = new() { IsAntialias = true };
    private float _cachedGlowRadius = float.NaN;

    public float Radius { get; set; } = MineRadius;

    public SKColor Color { get; set; } = new SKColor(0xD9, 0x78, 0x24);

    public float GlowRadius { get; set; } = 4f;

    public SKColor GlowColor { get; set; } = new SKColor(0xFF, 0xBF, 0x66);

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible || Alpha <= 0f)
            return;

        if (GlowRadius > 0f)
        {
            EnsureGlowMask();
            _glowPaint.Color = GlowColor.WithAlpha((byte)(55 * Alpha));
            canvas.DrawCircle(0f, 0f, Radius + GlowRadius / 2f, _glowPaint);
        }

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        canvas.DrawCircle(0f, 0f, Radius, _paint);
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
