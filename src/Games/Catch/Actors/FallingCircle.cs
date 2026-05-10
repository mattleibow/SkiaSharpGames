using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class FallingCircle : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPaint _glowPaint = new() { IsAntialias = true };
    private float _cachedGlowRadius = float.NaN;

    public FallingCircle()
    {
        Collider = new CircleCollider { Radius = CircleRadius };
        Rigidbody = new Rigidbody2D();
    }

    public new CircleCollider Collider
    {
        get => (CircleCollider)base.Collider!;
        init => base.Collider = value;
    }
    public new Rigidbody2D Rigidbody
    {
        get => (Rigidbody2D)base.Rigidbody!;
        init => base.Rigidbody = value;
    }

    public float Radius { get; set; } = CircleRadius;
    public SKColor Color { get; set; } = SKColors.White;
    public float GlowRadius { get; set; } = 12f;
    public SKColor GlowColor { get; set; } = AccentColor;

    protected override void OnDraw(SKCanvas canvas)
    {
        if (GlowRadius > 0f)
        {
            EnsureGlowMask();
            _glowPaint.Color = GlowColor.WithAlpha(60);
            canvas.DrawCircle(0f, 0f, Radius + GlowRadius / 2f, _glowPaint);
        }

        _paint.Color = Color;
        canvas.DrawCircle(0f, 0f, Radius, _paint);
    }

    private void EnsureGlowMask()
    {
        if (GlowRadius.Equals(_cachedGlowRadius))
            return;

        _glowPaint.MaskFilter?.Dispose();
        _glowPaint.MaskFilter =
            GlowRadius > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowRadius) : null;
        _cachedGlowRadius = GlowRadius;
    }
}
