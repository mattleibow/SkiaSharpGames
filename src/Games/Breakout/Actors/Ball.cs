using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// The player's ball actor.
/// Position (X, Y) is the centre of the circle.
/// </summary>
internal sealed class Ball : Actor
{
    private const float GlowRadius = 4f;

    private readonly SKPaint _paint = new() { IsAntialias = true };

    private readonly SKPaint _glowPaint = new()
    {
        IsAntialias = true,
        MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowRadius),
    };

    public float Radius { get; set; } = BallRadius;

    public SKColor Color { get; set; } = BallColor;

    public Ball()
    {
        Collider = new CircleCollider(BallRadius);
        Rigidbody = new Rigidbody2D();
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        _glowPaint.Color = Color.WithAlpha(60);
        canvas.DrawCircle(0, 0, Radius + GlowRadius / 2f, _glowPaint);

        _paint.Color = Color;
        canvas.DrawCircle(0, 0, Radius, _paint);
    }
}
