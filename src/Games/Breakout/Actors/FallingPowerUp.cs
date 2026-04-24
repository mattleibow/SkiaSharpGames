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

    private readonly HudLabel _label = new()
    {
        FontSize = 11f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        Y = 4f,
    };

    public float Width { get; set; } = PowerUpW;
    public float Height { get; set; } = PowerUpH;
    public SKColor Color { get; set; } = SKColors.White;
    public float CornerRadius { get; set; } = 5f;

    public FallingPowerUp()
    {
        Rigidbody = new Rigidbody2D { VelocityY = PowerUpSpeed };
        Collider = new RectCollider { Width = PowerUpW, Height = PowerUpH };
        Children.Add(_label);
    }

    public PowerUpType Type
    {
        get => _type;
        set
        {
            _type = value;
            _label.Text = value == PowerUpType.StrongBall ? "S" : "B";
        }
    }
    private PowerUpType _type;

    protected override void OnDraw(SKCanvas canvas)
    {
        _paint.Color = Color;
        canvas.DrawRoundRect(
            new SKRoundRect(
                SKRect.Create(0 - Width / 2f, 0 - Height / 2f, Width, Height),
                CornerRadius
            ),
            _paint
        );
    }
}
