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
    private readonly SKPaint _paint = new() { IsAntialias = true, Color = SKColors.White };

    private readonly HudLabel _label = new()
    {
        FontSize = 11f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        Y = 4f,
    };

    public FallingPowerUp()
    {
        Rigidbody = new Rigidbody2D { VelocityY = PowerUpSpeed };
        Collider = new RectCollider(PowerUpW, PowerUpH);
        Children.Add(_label);
    }

    public float Width { get; set; } = PowerUpW;

    public float Height { get; set; } = PowerUpH;

    public float CornerRadius { get; set; } = 5f;

    public SKColor Color
    {
        get => _paint.Color;
        set => _paint.Color = value;
    }

    public PowerUpType Type
    {
        get;
        set
        {
            field = value;
            _label.Text = value == PowerUpType.StrongBall ? "S" : "B";
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRoundRect(
            new SKRoundRect(
                SKRect.Create(0 - Width / 2f, 0 - Height / 2f, Width, Height),
                CornerRadius
            ),
            _paint
        );
    }
}
