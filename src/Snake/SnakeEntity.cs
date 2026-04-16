using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

/// <summary>The snake entity. Owns the body segment list and a sprite that renders it.</summary>
internal sealed class SnakeEntity : Entity
{
    private readonly LinkedList<GridPoint> _body = new();

    public SnakeEntity()
    {
        Sprite = new SnakeSprite { Body = _body };
        Collider = new RectCollider { Width = CellSize, Height = CellSize };
    }

    public IReadOnlyCollection<GridPoint> Body => _body;

    public GridPoint Head => _body.First!.Value;

    public int Length => _body.Count;

    public new SnakeSprite Sprite { get => (SnakeSprite)base.Sprite!; init => base.Sprite = value; }
    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }

    /// <summary>Reset the snake to its starting position in the centre of the grid.</summary>
    public void Reset()
    {
        _body.Clear();
        int startCol = GridCols / 2;
        int startRow = GridRows / 2;
        _body.AddFirst(new GridPoint(startCol, startRow));
        _body.AddLast(new GridPoint(startCol - 1, startRow));
        _body.AddLast(new GridPoint(startCol - 2, startRow));
        SyncPosition();
    }

    /// <summary>Advance the head in <paramref name="direction"/> and grow by one cell.</summary>
    public void Advance(Direction direction)
    {
        var head = Head;
        var next = direction switch
        {
            Direction.Up => new GridPoint(head.Col, head.Row - 1),
            Direction.Down => new GridPoint(head.Col, head.Row + 1),
            Direction.Left => new GridPoint(head.Col - 1, head.Row),
            Direction.Right => new GridPoint(head.Col + 1, head.Row),
            _ => head,
        };
        _body.AddFirst(next);
        SyncPosition();
    }

    /// <summary>Remove the tail segment (called when no food was eaten).</summary>
    public void TrimTail() => _body.RemoveLast();

    /// <summary>Returns true if any body segment occupies <paramref name="point"/>.</summary>
    public bool Occupies(GridPoint point)
    {
        foreach (var seg in _body)
            if (seg == point)
                return true;
        return false;
    }

    /// <summary>Returns true if the head has hit a wall.</summary>
    public bool IsOutOfBounds() =>
        Head.Col < 0 || Head.Col >= GridCols || Head.Row < 0 || Head.Row >= GridRows;

    /// <summary>Returns true if the head collides with any other body segment.</summary>
    public bool HasSelfCollision()
    {
        bool first = true;
        foreach (var seg in _body)
        {
            if (first) { first = false; continue; }
            if (seg == Head)
                return true;
        }
        return false;
    }

    private void SyncPosition()
    {
        // Keep the entity position on the head for collider/world-space queries
        X = Head.Col * CellSize + CellSize / 2f;
        Y = Head.Row * CellSize + CellSize / 2f;
    }
}
