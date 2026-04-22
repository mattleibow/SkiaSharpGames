using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

internal sealed class PlayScreen(SnakeGameState state, IDirector coordinator) : Scene
{
    private readonly GridEntity _grid = new();
    private readonly SnakeEntity _snake = new();
    private readonly FoodEntity _food = new();

    private readonly UiLabel _scoreText = new() { FontSize = 22f };
    private readonly UiLabel _highScoreText = new() { FontSize = 18f, Color = DimColor, Align = TextAlign.Right };

    private Direction _direction;
    private Direction _nextDirection;
    private float _stepTimer;
    private float _stepInterval;
    private bool _gameOverShown;

    public override void OnActivated()
    {
        state.Score = 0;
        _stepInterval = InitialStepInterval;
        _stepTimer = 0f;
        _gameOverShown = false;

        _snake.Reset();
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
        var head = _snake.Head;
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

        _snake.Advance(_direction);

        if (_snake.IsOutOfBounds() || _snake.HasSelfCollision())
        {
            Die();
            return;
        }

        if (_snake.Head == _food.Cell)
        {
            state.Score++;
            _stepInterval = MathF.Max(MinStepInterval, _stepInterval - SpeedIncrement);
            SpawnFood();
        }
        else
        {
            _snake.TrimTail();
        }
    }

    // ── Draw ──────────────────────────────────────────────────────────────

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        _grid.Draw(canvas);
        _food.Draw(canvas);
        _snake.Draw(canvas);

        // HUD
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

    private void SpawnFood()
    {
        GridPoint candidate;
        do
        {
            candidate = new GridPoint(
                Random.Shared.Next(GridCols),
                Random.Shared.Next(GridRows));
        }
        while (_snake.Occupies(candidate));

        _food.PlaceAt(candidate);
    }

    private static bool IsOpposite(Direction a, Direction b) =>
        (a == Direction.Up && b == Direction.Down) ||
        (a == Direction.Down && b == Direction.Up) ||
        (a == Direction.Left && b == Direction.Right) ||
        (a == Direction.Right && b == Direction.Left);
}
