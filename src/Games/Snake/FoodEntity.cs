using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

/// <summary>The food pickup. Positions itself on a grid cell and has a collider for overlap tests.</summary>
internal sealed class FoodEntity : Entity
{
    private static readonly SKPaint _paint = new() { IsAntialias = true };

    private const float Inset = 2f;
    private const float CornerRadius = 6f;

    public FoodEntity()
    {
        Collider = new RectCollider { Width = CellSize, Height = CellSize };
    }

    public GridPoint Cell { get; private set; }

    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f)
            return;

        float size = CellSize - Inset * 2f;
        _paint.Color = FoodColor.WithAlpha((byte)(255 * Alpha));
        canvas.DrawRoundRect(-size / 2f, -size / 2f, size, size, CornerRadius, CornerRadius, _paint);
    }

    /// <summary>Place the food on <paramref name="cell"/>.</summary>
    public void PlaceAt(GridPoint cell)
    {
        Cell = cell;
        X = cell.Col * CellSize + CellSize / 2f;
        Y = cell.Row * CellSize + CellSize / 2f;
    }
}
