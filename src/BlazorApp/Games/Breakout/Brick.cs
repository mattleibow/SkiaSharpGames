using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

internal sealed class Brick
{
    public readonly int Row, Col;
    public bool Active = true;
    public readonly RectSprite Sprite;
    public readonly RectBody Body;

    public Brick(int row, int col, float x, float y)
    {
        Row = row; Col = col;
        Sprite = new RectSprite
        {
            X = x, Y = y,
            Width = BreakoutConstants.BrickWidth, Height = BreakoutConstants.BrickHeight,
            CornerRadius = 3f, ShowShine = true
        };
        Body = new RectBody
        {
            X = x, Y = y,
            Width = BreakoutConstants.BrickWidth, Height = BreakoutConstants.BrickHeight,
            IsStatic = true
        };
    }
}
