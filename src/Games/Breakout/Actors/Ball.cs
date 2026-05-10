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

    private readonly SKPaint _paint = new() { IsAntialias = true, Color = BallColor };

    private readonly SKPaint _glowPaint = new()
    {
        IsAntialias = true,
        MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowRadius),
        Color = BallColor,
    };

    public float Radius { get; set; } = BallRadius;

    public SKColor Color
    {
        get => _paint.Color;
        set
        {
            _paint.Color = value;
            _glowPaint.Color = value.WithAlpha(60);
        }
    }

    public Ball()
    {
        Collider = new CircleCollider(BallRadius);
        Rigidbody = new Rigidbody2D();
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawCircle(0, 0, Radius + GlowRadius / 2f, _glowPaint);
        canvas.DrawCircle(0, 0, Radius, _paint);
    }
}
