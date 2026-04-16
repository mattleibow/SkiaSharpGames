using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

/// <summary>The food pickup. Positions itself on a grid cell and has a collider for overlap tests.</summary>
internal sealed class FoodEntity : Entity
{
    public FoodEntity()
    {
        Sprite = new CellSprite { Color = FoodColor, Inset = 2f, CornerRadius = 6f };
        Collider = new RectCollider { Width = CellSize, Height = CellSize };
    }

    public GridPoint Cell { get; private set; }

    public new CellSprite Sprite { get => (CellSprite)base.Sprite!; init => base.Sprite = value; }
    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }

    /// <summary>Place the food on <paramref name="cell"/>.</summary>
    public void PlaceAt(GridPoint cell)
    {
        Cell = cell;
        X = cell.Col * CellSize + CellSize / 2f;
        Y = cell.Row * CellSize + CellSize / 2f;
    }
}
