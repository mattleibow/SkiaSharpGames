using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

internal sealed class Brick : Entity
{
    public readonly int Row, Col;

    public Brick(int row, int col, float cx, float cy)
    {
        Row = row;
        Col = col;
        X = cx;
        Y = cy;
        Collider = new RectCollider { Width = BrickWidth, Height = BrickHeight };
        Sprite = new BrickSprite { Width = BrickWidth, Height = BrickHeight };
    }

    public new BrickSprite Sprite { get => (BrickSprite)base.Sprite!; init => base.Sprite = value; }
    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }
}

