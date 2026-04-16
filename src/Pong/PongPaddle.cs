using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class PongPaddle : Entity
{
    public PongPaddle(SKColor color)
    {
        Collider = new RectCollider
        {
            Width = PaddleWidth,
            Height = PaddleHeight,
        };
        Sprite = new PaddleSprite
        {
            Width = PaddleWidth,
            Height = PaddleHeight,
            Color = color,
            CornerRadius = 5f,
        };
    }

    public bool UpHeld { get; set; }
    public bool DownHeld { get; set; }

    public new PaddleSprite Sprite { get => (PaddleSprite)base.Sprite!; init => base.Sprite = value; }
    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }

    protected override void OnUpdate(float deltaTime)
    {
        float move = 0f;
        if (UpHeld) move -= PaddleSpeed * deltaTime;
        if (DownHeld) move += PaddleSpeed * deltaTime;
        if (move != 0f)
            Y = ClampPaddleY(Y + move);
    }

    private static float ClampPaddleY(float y) =>
        Math.Clamp(y, PaddleHeight * 0.5f, GameHeight - PaddleHeight * 0.5f);
}
