using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class Invader : Entity
{
    public int Row { get; }
    public int Col { get; }

    public Invader(int row, int col)
    {
        Row = row;
        Col = col;
        Collider = new RectCollider { Width = InvaderWidth, Height = InvaderHeight };
        Sprite = new InvaderSprite { Row = row };
    }

    public new InvaderSprite Sprite { get => (InvaderSprite)base.Sprite!; init => base.Sprite = value; }

    public int ScoreValue =>
        Row switch
        {
            0 => 30,
            1 or 2 => 20,
            _ => 10
        };
}
