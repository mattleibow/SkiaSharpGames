using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class PongPaddle(SKColor color) : Entity
{
    public readonly RectCollider Collider = new()
    {
        Width = PaddleWidth,
        Height = PaddleHeight,
    };

    public readonly PaddleSprite Sprite = new()
    {
        Width = PaddleWidth,
        Height = PaddleHeight,
        Color = color,
        CornerRadius = 5f,
    };
}
