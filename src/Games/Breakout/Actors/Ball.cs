using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// The player's ball actor. Position (X, Y) is the centre of the circle.
/// </summary>
internal sealed class Ball : Actor
{
    private const float GlowRadius = 4f;

    private readonly SKPaint _paint = new()
    {
        IsAntialias = true
    };
    
    private readonly SKPaint _glowPaint = new()
    {
        IsAntialias = true,
        MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowRadius);
    };

    public float Radius { get; set; } = BallRadius;

    public SKColor Color { get; set; } = SKColors.White;

    public Ball()
    {
        Collider = new CircleCollider { Radius = BallRadius };
        Rigidbody = new Rigidbody2D();
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f)
            return;

        _glowPaint.Color = Color.WithAlpha((byte)(60 * Alpha));
        canvas.DrawCircle(0, 0, Radius + GlowRadius / 2f, _glowPaint);

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        canvas.DrawCircle(0, 0, Radius, _paint);
    }
}
