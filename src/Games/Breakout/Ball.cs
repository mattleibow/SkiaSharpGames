using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// The player's ball entity. Position (X, Y) is the centre of the circle.
/// </summary>
internal sealed class Ball : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPaint _glowPaint = new() { IsAntialias = true };
    private float _cachedGlowRadius = float.NaN;

    public float Radius { get; set; } = BallRadius;
    public SKColor Color { get; set; } = SKColors.White;
    public float GlowRadius { get; set; } = 4f;
    public SKColor GlowColor { get; set; } = SKColors.White;

    public Ball()
    {
        Collider = new CircleCollider { Radius = BallRadius };
        Rigidbody = new Rigidbody2D();
    }

    public new CircleCollider Collider { get => (CircleCollider)base.Collider!; init => base.Collider = value; }
    public new Rigidbody2D Rigidbody { get => (Rigidbody2D)base.Rigidbody!; init => base.Rigidbody = value; }

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f)
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
