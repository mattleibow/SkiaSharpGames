using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

internal sealed class PlayScreen(SnakeGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _cellPaint = new() { IsAntialias = true };
    private static readonly SKPaint _gridPaint = new() { Color = GridLineColor, StrokeWidth = 1f, IsAntialias = true };

    private readonly TextSprite _scoreText = new() { Size = 22f };
    private readonly TextSprite _highScoreText = new() { Size = 18f, Color = DimColor, Align = TextAlign.Right };

    private readonly LinkedList<GridPoint> _body = new();
    private Direction _direction;
    private Direction _nextDirection;
    private GridPoint _food;
    private float _stepTimer;
    private float _stepInterval;
    private bool _gameOverShown;

    public override void OnActivated()
    {
        state.Score = 0;
        _stepInterval = InitialStepInterval;
        _stepTimer = 0f;
        _gameOverShown = false;

        _body.Clear();
        int startCol = GridCols / 2;
        int startRow = GridRows / 2;
        _body.AddFirst(new GridPoint(startCol, startRow));
        _body.AddLast(new GridPoint(startCol - 1, startRow));
        _body.AddLast(new GridPoint(startCol - 2, startRow));

        _direction = Direction.Right;
        _nextDirection = Direction.Right;

        SpawnFood();
    }

    // ── Input ─────────────────────────────────────────────────────────────

    public override void OnKeyDown(string key)
    {
        var desired = key switch
        {
            "ArrowUp" or "w" or "W" => Direction.Up,
            "ArrowDown" or "s" or "S" => Direction.Down,
            "ArrowLeft" or "a" or "A" => Direction.Left,
            "ArrowRight" or "d" or "D" => Direction.Right,
            _ => (Direction?)null,
        };

        if (desired is not null && !IsOpposite(desired.Value, _direction))
            _nextDirection = desired.Value;
    }

    public override void OnPointerDown(float x, float y)
    {
        var head = _body.First!.Value;
        float hx = head.Col * CellSize + CellSize / 2f;
        float hy = head.Row * CellSize + CellSize / 2f;

        float dx = x - hx;
        float dy = y - hy;

        Direction desired;
        if (MathF.Abs(dx) > MathF.Abs(dy))
            desired = dx > 0 ? Direction.Right : Direction.Left;
        else
            desired = dy > 0 ? Direction.Down : Direction.Up;

        if (!IsOpposite(desired, _direction))
            _nextDirection = desired;
    }

    // ── Update ────────────────────────────────────────────────────────────

    public override void Update(float deltaTime)
    {
        _stepTimer += deltaTime;

        if (_stepTimer < _stepInterval)
            return;

        _stepTimer -= _stepInterval;
        _direction = _nextDirection;

        var head = _body.First!.Value;
        var next = _direction switch
        {
            Direction.Up => new GridPoint(head.Col, head.Row - 1),
            Direction.Down => new GridPoint(head.Col, head.Row + 1),
            Direction.Left => new GridPoint(head.Col - 1, head.Row),
            Direction.Right => new GridPoint(head.Col + 1, head.Row),
            _ => head,
        };

        // Wall collision
        if (next.Col < 0 || next.Col >= GridCols || next.Row < 0 || next.Row >= GridRows)
        {
            Die();
            return;
        }

        // Self collision
        if (BodyContains(next))
        {
            Die();
            return;
        }

        _body.AddFirst(next);

        if (next == _food)
        {
            state.Score++;
            _stepInterval = MathF.Max(MinStepInterval, _stepInterval - SpeedIncrement);
            SpawnFood();
        }
        else
        {
            _body.RemoveLast();
        }
    }

    // ── Draw ──────────────────────────────────────────────────────────────

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        DrawGrid(canvas);
        DrawFood(canvas);
        DrawSnake(canvas);
        DrawHud(canvas);
    }

    private static void DrawGrid(SKCanvas canvas)
    {
        for (int c = 1; c < GridCols; c++)
        {
            float x = c * CellSize;
            canvas.DrawLine(x, 0, x, GameHeight, _gridPaint);
        }
        for (int r = 1; r < GridRows; r++)
        {
            float y = r * CellSize;
            canvas.DrawLine(0, y, GameWidth, y, _gridPaint);
        }
    }

    private void DrawFood(SKCanvas canvas)
    {
        _cellPaint.Color = FoodColor;
        float fx = _food.Col * CellSize + 2f;
        float fy = _food.Row * CellSize + 2f;
        canvas.DrawRoundRect(fx, fy, CellSize - 4f, CellSize - 4f, 6f, 6f, _cellPaint);
    }

    private void DrawSnake(SKCanvas canvas)
    {
        bool isHead = true;
        foreach (var seg in _body)
        {
            _cellPaint.Color = isHead ? SnakeHeadColor : SnakeBodyColor;
            float sx = seg.Col * CellSize + 1f;
            float sy = seg.Row * CellSize + 1f;
            canvas.DrawRoundRect(sx, sy, CellSize - 2f, CellSize - 2f, 4f, 4f, _cellPaint);
            isHead = false;
        }
    }

    private void DrawHud(SKCanvas canvas)
    {
        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(10f, -6f); _scoreText.Draw(canvas); canvas.Restore();

        if (state.HighScore > 0)
        {
            _highScoreText.Text = $"Best: {state.HighScore}";
            canvas.Save(); canvas.Translate(GameWidth - 10f, -6f); _highScoreText.Draw(canvas); canvas.Restore();
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private void Die()
    {
        if (_gameOverShown) return;
        _gameOverShown = true;

        if (state.Score > state.HighScore)
            state.HighScore = state.Score;

        coordinator.PushOverlay<GameOverScreen>();
    }

    private bool BodyContains(GridPoint point)
    {
        foreach (var seg in _body)
        {
            if (seg == point)
                return true;
        }
        return false;
    }

    private void SpawnFood()
    {
        GridPoint candidate;
        do
        {
            candidate = new GridPoint(
                Random.Shared.Next(GridCols),
                Random.Shared.Next(GridRows));
        }
        while (BodyContains(candidate));

        _food = candidate;
    }

    private static bool IsOpposite(Direction a, Direction b) =>
        (a == Direction.Up && b == Direction.Down) ||
        (a == Direction.Down && b == Direction.Up) ||
        (a == Direction.Left && b == Direction.Right) ||
        (a == Direction.Right && b == Direction.Left);
}
