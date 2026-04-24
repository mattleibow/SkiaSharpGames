using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

internal enum PowerUpType
{
    StrongBall,
    BigPaddle,
}

internal sealed class FallingPowerUp : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };

    public PowerUpType Type;

    public float Width { get; set; } = PowerUpW;
    public float Height { get; set; } = PowerUpH;
    public SKColor Color { get; set; } = SKColors.White;
    public float CornerRadius { get; set; } = 5f;

    public FallingPowerUp()
    {
        Rigidbody = new Rigidbody2D { VelocityY = PowerUpSpeed };
        Collider = new RectCollider { Width = PowerUpW, Height = PowerUpH };
    }

    public new RectCollider Collider
    {
        get => (RectCollider)base.Collider!;
        init => base.Collider = value;
    }
    public new Rigidbody2D Rigidbody
    {
        get => (Rigidbody2D)base.Rigidbody!;
        init => base.Rigidbody = value;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f)
            return;

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        canvas.DrawRoundRect(
            new SKRoundRect(
                SKRect.Create(0 - Width / 2f, 0 - Height / 2f, Width, Height),
                CornerRadius
            ),
            _paint
        );
    }
}