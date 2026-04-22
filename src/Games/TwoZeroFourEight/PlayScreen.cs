using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.TwoZeroFourEight.TwoZeroFourEightConstants;

namespace SkiaSharpGames.TwoZeroFourEight;

internal sealed class PlayScreen(TwoZeroFourEightGameState state, IDirector coordinator) : Scene
{
    private const float SlideDuration = 0.12f;
    private const float SpawnDuration = 0.16f;
    private const float MaxTileScale = 1.15f;

    private static readonly SKPaint _boardPaint = new() { Color = BoardColor, IsAntialias = true };
    private static readonly SKPaint _tilePaint = new() { IsAntialias = true };

    private readonly UiLabel _headerText = new() { Text = "2048", FontSize = 68f, Color = HeaderColor };
    private readonly UiLabel _scoreText = new() { FontSize = 24f, Color = HeaderColor };
    private readonly UiLabel _bestText = new() { FontSize = 24f, Color = HeaderColor };
    private readonly UiLabel _controlsText = new() { Text = "Arrow keys / WASD / swipe to move", FontSize = 20f, Color = HeaderColor, Align = TextAlign.Center };
    private readonly UiLabel _footerText = new() { FontSize = 22f, Color = HeaderColor, Align = TextAlign.Center };
    private readonly UiLabel _tileText = new();

    private readonly int[,] _board = new int[GridSize, GridSize];
    private readonly Random _random = Random.Shared;
    private readonly List<SlideAnimation> _slideTiles = [];
    private readonly HashSet<int> _slideSourceCells = [];

    private int[,]? _pendingBoard;
    private int _pendingScoreGain;
    private bool _slideActive;
    private float _slideProgress;

    private SpawnAnimation? _spawnTile;

    private bool _gameOverShown;
    private float _pointerStartX;
    private float _pointerStartY;

    private bool IsSpawnAnimating => _spawnTile is { Progress: < 1f };

    private bool IsAnimating => _slideActive || IsSpawnAnimating;

    public override void OnActivated()
    {
        Array.Clear(_board);
        state.Score = 0;
        state.HasReached2048 = false;

        _gameOverShown = false;
        _slideTiles.Clear();
        _slideSourceCells.Clear();
        _pendingBoard = null;
        _pendingScoreGain = 0;
        _slideActive = false;
        _slideProgress = 0f;
        _spawnTile = null;

        SpawnRandomTile(out _, out _, out _);
        SpawnRandomTile(out _, out _, out _);
    }

    public override void Update(float deltaTime)
    {
        if (_slideActive)
        {
            _slideProgress = MathF.Min(1f, _slideProgress + deltaTime / SlideDuration);
            if (_slideProgress >= 1f)
                FinalizeSlideMove();
        }

        if (_spawnTile is { } spawn && spawn.Progress < 1f)
        {
            spawn.Progress = MathF.Min(1f, spawn.Progress + deltaTime / SpawnDuration);
            _spawnTile = spawn;
        }
    }

    public override void OnPointerDown(float x, float y)
    {
        _pointerStartX = x;
        _pointerStartY = y;
    }

    public override void OnPointerUp(float x, float y)
    {
        if (IsAnimating)
            return;

        float dx = x - _pointerStartX;
        float dy = y - _pointerStartY;

        if (MathF.Abs(dx) < 20f && MathF.Abs(dy) < 20f)
            return;

        if (MathF.Abs(dx) > MathF.Abs(dy))
            TryMove(dx > 0 ? MoveDirection.Right : MoveDirection.Left);
        else
            TryMove(dy > 0 ? MoveDirection.Down : MoveDirection.Up);
    }

    public override void OnKeyDown(string key)
    {
        switch (key)
        {
            case "r":
            case "R":
                coordinator.TransitionTo<PlayScreen>(new DissolveCurtain());
                return;
        }

        if (IsAnimating)
            return;

        switch (key)
        {
            case "ArrowLeft":
            case "a":
            case "A":
                TryMove(MoveDirection.Left);
                break;
            case "ArrowRight":
            case "d":
            case "D":
                TryMove(MoveDirection.Right);
                break;
            case "ArrowUp":
            case "w":
            case "W":
                TryMove(MoveDirection.Up);
                break;
            case "ArrowDown":
            case "s":
            case "S":
                TryMove(MoveDirection.Down);
                break;
        }
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        canvas.Save(); canvas.Translate(90f, 94f); _headerText.Draw(canvas); canvas.Restore();

        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(GameWidth - 280f, 72f); _scoreText.Draw(canvas); canvas.Restore();

        _bestText.Text = $"Best: {state.BestScore}";
        canvas.Save(); canvas.Translate(GameWidth - 280f, 102f); _bestText.Draw(canvas); canvas.Restore();

        canvas.Save(); canvas.Translate(GameWidth / 2f, 64f); _controlsText.Draw(canvas); canvas.Restore();

        var boardRect = SKRect.Create(BoardX, BoardY, BoardPixelSize, BoardPixelSize);
        canvas.DrawRoundRect(boardRect, 18f, 18f, _boardPaint);

        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                int value = _board[row, col];

                if (_slideActive && _slideSourceCells.Contains(CellKey(row, col)))
                    value = 0;

                if (IsSpawnAnimating && _spawnTile is { } spawn && spawn.Row == row && spawn.Col == col)
                    value = 0;

                DrawTile(canvas, row, col, value);
            }
        }

        if (_slideActive)
            DrawSlideTiles(canvas);

        if (IsSpawnAnimating && _spawnTile is { } spawnTile)
            DrawTile(canvas, spawnTile.Row, spawnTile.Col, spawnTile.Value, Easing.BackOut(spawnTile.Progress));

        _footerText.Text = state.HasReached2048
            ? "2048 reached! Keep going..."
            : "Merge tiles to reach 2048";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 565f); _footerText.Draw(canvas); canvas.Restore();
    }

    private void DrawSlideTiles(SKCanvas canvas)
    {
        float t = Easing.EaseOut(_slideProgress);
        foreach (var tile in _slideTiles)
        {
            float fromX = TileX(tile.FromCol);
            float fromY = TileY(tile.FromRow);
            float toX = TileX(tile.ToCol);
            float toY = TileY(tile.ToRow);

            float x = fromX + (toX - fromX) * t;
            float y = fromY + (toY - fromY) * t;
            DrawTileAt(canvas, x, y, tile.Value, 1f);
        }
    }

    private static float TileX(int col) => BoardX + BoardPadding + col * (TileSize + TileGap);

    private static float TileY(int row) => BoardY + BoardPadding + row * (TileSize + TileGap);

    private static int CellKey(int row, int col) => row * GridSize + col;

    private void DrawTile(SKCanvas canvas, int row, int col, int value, float scale = 1f) =>
        DrawTileAt(canvas, TileX(col), TileY(row), value, scale);

    private void DrawTileAt(SKCanvas canvas, float x, float y, int value, float scale)
    {
        float clampedScale = Math.Clamp(scale, 0f, MaxTileScale);
        float size = TileSize * clampedScale;
        float left = x + (TileSize - size) / 2f;
        float top = y + (TileSize - size) / 2f;

        _tilePaint.Color = value == 0 ? EmptyTileColor : GetTileColor(value);

        var tileRect = SKRect.Create(left, top, size, size);
        canvas.DrawRoundRect(tileRect, 12f * clampedScale, 12f * clampedScale, _tilePaint);

        if (value == 0)
            return;

        string text = value.ToString();
        float textSize = value switch
        {
            < 100 => 44f,
            < 1000 => 40f,
            < 10000 => 34f,
            _ => 28f
        };

        textSize *= clampedScale;

        _tileText.Text = text;
        _tileText.FontSize = textSize;
        _tileText.Color = value <= 4 ? DarkTextColor : LightTextColor;
        _tileText.Align = TextAlign.Center;
        canvas.Save(); canvas.Translate(left + size / 2f, top + size / 2f + textSize / 3f); _tileText.Draw(canvas); canvas.Restore();
    }

    private void TryMove(MoveDirection direction)
    {
        if (IsAnimating)
            return;

        if (!ApplyMove(direction, out var nextBoard, out var moves, out int gained))
            return;

        _pendingBoard = nextBoard;
        _pendingScoreGain = gained;

        _slideTiles.Clear();
        _slideSourceCells.Clear();
        foreach (var move in moves)
        {
            _slideTiles.Add(move);
            _slideSourceCells.Add(CellKey(move.FromRow, move.FromCol));
        }

        _spawnTile = null;
        _slideActive = true;
        _slideProgress = 0f;

        if (_slideTiles.Count == 0)
            FinalizeSlideMove();
    }

    private void FinalizeSlideMove()
    {
        if (_pendingBoard is null)
            return;

        CopyBoard(_pendingBoard, _board);

        _pendingBoard = null;
        _slideActive = false;
        _slideProgress = 0f;
        _slideTiles.Clear();
        _slideSourceCells.Clear();

        state.Score += _pendingScoreGain;
        _pendingScoreGain = 0;

        if (state.Score > state.BestScore)
            state.BestScore = state.Score;

        if (SpawnRandomTile(out int row, out int col, out int value))
            _spawnTile = new SpawnAnimation(row, col, value, 0f);

        if (!state.HasReached2048 && HasTile(2048))
            state.HasReached2048 = true;

        if (!HasMovesLeft() && !_gameOverShown)
        {
            _gameOverShown = true;
            coordinator.PushOverlay<GameOverScreen>();
        }
    }

    private bool ApplyMove(MoveDirection direction, out int[,] nextBoard, out List<SlideAnimation> moves, out int gained)
    {
        nextBoard = new int[GridSize, GridSize];
        moves = [];
        gained = 0;
        bool changed = false;

        for (int i = 0; i < GridSize; i++)
        {
            int[] original = new int[GridSize];
            var sourceValues = new List<(int Value, int SourceIndex)>(GridSize);

            for (int j = 0; j < GridSize; j++)
            {
                int value = GetLineCell(_board, direction, i, j);
                original[j] = value;
                if (value != 0)
                    sourceValues.Add((value, j));
            }

            int[] result = new int[GridSize];
            int destIndex = 0;

            for (int s = 0; s < sourceValues.Count; s++)
            {
                var current = sourceValues[s];

                if (s + 1 < sourceValues.Count && sourceValues[s + 1].Value == current.Value)
                {
                    int merged = current.Value * 2;
                    result[destIndex] = merged;
                    gained += merged;

                    AddMoveAnimation(moves, direction, i, current.SourceIndex, destIndex, current.Value);
                    AddMoveAnimation(moves, direction, i, sourceValues[s + 1].SourceIndex, destIndex, sourceValues[s + 1].Value);

                    destIndex++;
                    s++;
                }
                else
                {
                    result[destIndex] = current.Value;
                    AddMoveAnimation(moves, direction, i, current.SourceIndex, destIndex, current.Value);
                    destIndex++;
                }
            }

            for (int j = 0; j < GridSize; j++)
                SetLineCell(nextBoard, direction, i, j, result[j]);

            if (!changed)
                changed = !original.SequenceEqual(result);
        }

        return changed;
    }

    private static void AddMoveAnimation(List<SlideAnimation> moves, MoveDirection direction, int fixedIndex, int fromIndex, int toIndex, int value)
    {
        var from = ResolveCell(direction, fixedIndex, fromIndex);
        var to = ResolveCell(direction, fixedIndex, toIndex);

        if (from.Row == to.Row && from.Col == to.Col)
            return;

        moves.Add(new SlideAnimation(from.Row, from.Col, to.Row, to.Col, value));
    }

    // Converts a line coordinate (fixed index + move index in move direction order)
    // back into a concrete board cell.
    private static (int Row, int Col) ResolveCell(MoveDirection direction, int fixedIndex, int moveIndex) => direction switch
    {
        MoveDirection.Left => (fixedIndex, moveIndex),
        MoveDirection.Right => (fixedIndex, GridSize - 1 - moveIndex),
        MoveDirection.Up => (moveIndex, fixedIndex),
        MoveDirection.Down => (GridSize - 1 - moveIndex, fixedIndex),
        _ => (0, 0)
    };

    private static int GetLineCell(int[,] board, MoveDirection direction, int fixedIndex, int moveIndex)
    {
        var cell = ResolveCell(direction, fixedIndex, moveIndex);
        return board[cell.Row, cell.Col];
    }

    private static void SetLineCell(int[,] board, MoveDirection direction, int fixedIndex, int moveIndex, int value)
    {
        var cell = ResolveCell(direction, fixedIndex, moveIndex);
        board[cell.Row, cell.Col] = value;
    }

    private static void CopyBoard(int[,] source, int[,] destination)
    {
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
                destination[row, col] = source[row, col];
        }
    }

    private bool SpawnRandomTile(out int spawnedRow, out int spawnedCol, out int spawnedValue)
    {
        var empty = new List<(int row, int col)>();
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                if (_board[row, col] == 0)
                    empty.Add((row, col));
            }
        }

        if (empty.Count == 0)
        {
            spawnedRow = 0;
            spawnedCol = 0;
            spawnedValue = 0;
            return false;
        }

        var (tileRow, tileCol) = empty[_random.Next(empty.Count)];
        int value = _random.NextDouble() < 0.9 ? 2 : 4;
        _board[tileRow, tileCol] = value;

        spawnedRow = tileRow;
        spawnedCol = tileCol;
        spawnedValue = value;
        return true;
    }

    private bool HasTile(int target)
    {
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                if (_board[row, col] == target)
                    return true;
            }
        }

        return false;
    }

    private bool HasMovesLeft()
    {
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                int value = _board[row, col];
                if (value == 0)
                    return true;

                if (row + 1 < GridSize && _board[row + 1, col] == value)
                    return true;

                if (col + 1 < GridSize && _board[row, col + 1] == value)
                    return true;
            }
        }

        return false;
    }

    private static SKColor GetTileColor(int value) => value switch
    {
        2 => new SKColor(0xEE, 0xE4, 0xDA),
        4 => new SKColor(0xED, 0xE0, 0xC8),
        8 => new SKColor(0xF2, 0xB1, 0x79),
        16 => new SKColor(0xF5, 0x95, 0x63),
        32 => new SKColor(0xF6, 0x7C, 0x5F),
        64 => new SKColor(0xF6, 0x5E, 0x3B),
        128 => new SKColor(0xED, 0xCF, 0x72),
        256 => new SKColor(0xED, 0xCC, 0x61),
        512 => new SKColor(0xED, 0xC8, 0x50),
        1024 => new SKColor(0xED, 0xC5, 0x3F),
        2048 => new SKColor(0xED, 0xC2, 0x2E),
        _ => new SKColor(0x3C, 0x3A, 0x32)
    };

    private enum MoveDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    private readonly record struct SlideAnimation(int FromRow, int FromCol, int ToRow, int ToCol, int Value);

    private record struct SpawnAnimation(int Row, int Col, int Value, float Progress);
}
