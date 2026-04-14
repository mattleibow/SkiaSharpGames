using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.Breakout.BreakoutConstants;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

/// <summary>
/// The player's paddle entity. Position (X, Y) is the centre of the rectangle.
/// </summary>
internal sealed class Paddle : Entity
{
    public readonly RectCollider Collider = new()
    {
        Width = DefaultPaddleWidth,
        Height = PaddleHeight,
    };
    public readonly RectSprite Sprite = new()
    {
        Height = PaddleHeight,
        Color = PaddleColor,
        CornerRadius = 6f,
        ShowShine = false,
    };
}
