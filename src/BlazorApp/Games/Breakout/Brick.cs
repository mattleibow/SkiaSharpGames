using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.Breakout.BreakoutConstants;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

internal sealed class Brick : Entity
{
    public readonly int Row, Col;
    public readonly RectCollider Collider = new()
    {
        Width = BrickWidth,
        Height = BrickHeight,
    };
    public readonly RectSprite Sprite = new()
    {
        Width = BrickWidth,
        Height = BrickHeight,
        CornerRadius = 3f,
        ShowShine = true,
    };

    /// <param name="row">Row index.</param>
    /// <param name="col">Column index.</param>
    /// <param name="cx">Centre X in game-space units.</param>
    /// <param name="cy">Centre Y in game-space units.</param>
    public Brick(int row, int col, float cx, float cy)
    {
        Row = row;
        Col = col;
        X = cx;
        Y = cy;
    }
}

