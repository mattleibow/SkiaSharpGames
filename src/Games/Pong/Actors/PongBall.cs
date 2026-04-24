using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class PongBall : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPaint _glowPaint = new()
    {
        IsAntialias = true,
        MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2f),
    };

    public float Radius { get; set; } = BallRadius;
    public SKColor Color { get; set; } = SKColors.White;
    public float GlowRadius { get; set; } = 2f;
    public SKColor GlowColor { get; set; } = SKColors.White;

    public PongBall()
    {
        Collider = new CircleCollider { Radius = BallRadius };
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

    protected override void OnDraw(SKCanvas canvas)
    {
        if (GlowRadius > 0f)
        {
            _glowPaint.Color = GlowColor.WithAlpha(127);
            canvas.DrawCircle(0f, 0f, Radius + GlowRadius, _glowPaint);
        }

        _paint.Color = Color;
        canvas.DrawCircle(0f, 0f, Radius, _paint);
    }
}
