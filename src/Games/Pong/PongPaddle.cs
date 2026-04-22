using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class PongPaddle : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };

    public float Width { get; set; } = PaddleWidth;
    public float Height { get; set; } = PaddleHeight;
    public SKColor Color { get; set; }
    public float CornerRadius { get; set; } = 5f;

    public PongPaddle(SKColor color)
    {
        Color = color;
        Collider = new RectCollider
        {
            Width = PaddleWidth,
            Height = PaddleHeight,
        };
    }

    public bool UpHeld { get; set; }
    public bool DownHeld { get; set; }

    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }

    protected override void OnUpdate(float deltaTime)
    {
        float move = 0f;
        if (UpHeld) move -= PaddleSpeed * deltaTime;
        if (DownHeld) move += PaddleSpeed * deltaTime;
        if (move != 0f)
            Y = ClampPaddleY(Y + move);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f)
            return;

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        var rect = SKRect.Create(-Width / 2f, -Height / 2f, Width, Height);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _paint);
    }

    private static float ClampPaddleY(float y) =>
        Math.Clamp(y, PaddleHeight * 0.5f, GameHeight - PaddleHeight * 0.5f);
}
