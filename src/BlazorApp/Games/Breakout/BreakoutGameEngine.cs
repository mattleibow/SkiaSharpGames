using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

public enum GamePhase { Start, Playing, GameOver, Victory }

public class BreakoutGameEngine : GameScreenBase
{
    // Game dimensions (logical/virtual coordinates)
    public const int GameWidth = 800;
    public const int GameHeight = 600;

    // Paddle
    private const float PaddleWidth = 100f;
    private const float PaddleHeight = 14f;
    private const float PaddleY = 558f;

    // Ball
    private const float BallRadius = 8f;
    private const float BallSpeed = 350f;

    // Bricks
    private const int BrickCols = 10;
    private const int BrickRows = 5;
    private const float BrickWidth = 72f;
    private const float BrickHeight = 22f;
    private const float BrickGap = 4f;
    private static readonly float BricksStartX = (GameWidth - (BrickCols * (BrickWidth + BrickGap) - BrickGap)) / 2f;
    private const float BricksStartY = 60f;

    public override (int width, int height) GameDimensions => (GameWidth, GameHeight);

    // State
    public GamePhase Phase { get; private set; } = GamePhase.Start;
    public int Score { get; private set; }
    public int Lives { get; private set; } = 3;

    private float _paddleX = (GameWidth - PaddleWidth) / 2f;
    private float _ballX = GameWidth / 2f;
    private float _ballY = GameHeight / 2f;
    private float _ballVX = 0f;
    private float _ballVY = 0f;
    private bool[,] _bricks = new bool[BrickRows, BrickCols];

    // Colors for brick rows
    private static readonly SKColor[] BrickColors =
    [
        new SKColor(0xFF, 0x2D, 0x55), // Red   (row 0 – top, highest score)
        new SKColor(0xFF, 0x9F, 0x0A), // Orange
        new SKColor(0xFF, 0xD6, 0x0A), // Yellow
        new SKColor(0x30, 0xD1, 0x58), // Green
        new SKColor(0x0A, 0x84, 0xFF), // Blue  (row 4 – bottom)
    ];

    private static readonly SKColor BackgroundColor = new(0x0D, 0x1B, 0x2A);
    private static readonly SKColor PaddleColor = new(0x00, 0xD4, 0xFF);
    private static readonly SKColor AccentColor = new(0x00, 0xD4, 0xFF);
    private static readonly SKColor DimColor = new(0xAA, 0xAA, 0xAA);

    public void StartGame()
    {
        Phase = GamePhase.Playing;
        Score = 0;
        Lives = 3;
        InitializeBricks();
        _paddleX = (GameWidth - PaddleWidth) / 2f;
        ResetBall();
    }

    private void InitializeBricks()
    {
        for (int r = 0; r < BrickRows; r++)
            for (int c = 0; c < BrickCols; c++)
                _bricks[r, c] = true;
    }

    private void ResetBall()
    {
        _ballX = _paddleX + PaddleWidth / 2f;
        _ballY = PaddleY - BallRadius - 10f;

        // Launch at a random upward angle between 35° and 145°
        double angle = (35.0 + Random.Shared.NextDouble() * 110.0) * Math.PI / 180.0;
        _ballVX = (float)(BallSpeed * Math.Cos(angle));
        _ballVY = -(float)(BallSpeed * Math.Sin(angle));
    }

    public override void OnPointerMove(float x, float y)
    {
        if (Phase == GamePhase.Playing)
            _paddleX = Math.Clamp(x - PaddleWidth / 2f, 0f, GameWidth - PaddleWidth);
    }

    public override void OnPointerDown(float x, float y)
    {
        if (Phase is GamePhase.Start or GamePhase.GameOver or GamePhase.Victory)
            StartGame();
    }

    public override void Update(float deltaTime)
    {
        if (Phase != GamePhase.Playing)
            return;

        // Move ball
        _ballX += _ballVX * deltaTime;
        _ballY += _ballVY * deltaTime;

        // Left / right wall
        if (_ballX - BallRadius < 0f)
        {
            _ballX = BallRadius;
            _ballVX = MathF.Abs(_ballVX);
        }
        else if (_ballX + BallRadius > GameWidth)
        {
            _ballX = GameWidth - BallRadius;
            _ballVX = -MathF.Abs(_ballVX);
        }

        // Top wall
        if (_ballY - BallRadius < 0f)
        {
            _ballY = BallRadius;
            _ballVY = MathF.Abs(_ballVY);
        }

        // Ball lost below the screen
        if (_ballY > GameHeight + BallRadius * 2)
        {
            Lives--;
            if (Lives <= 0)
                Phase = GamePhase.GameOver;
            else
                ResetBall();
            return;
        }

        // Paddle collision
        if (_ballVY > 0f &&
            _ballX >= _paddleX && _ballX <= _paddleX + PaddleWidth &&
            _ballY + BallRadius >= PaddleY && _ballY + BallRadius <= PaddleY + PaddleHeight + 6f)
        {
            _ballVY = -MathF.Abs(_ballVY);

            // Angle based on hit position along paddle (-1 … +1)
            float hitPos = (_ballX - (_paddleX + PaddleWidth / 2f)) / (PaddleWidth / 2f);
            float maxAngle = 65f * MathF.PI / 180f;
            float angle = hitPos * maxAngle;
            _ballVX = BallSpeed * MathF.Sin(angle);
            _ballVY = -BallSpeed * MathF.Cos(angle);
        }

        // Brick collisions
        CheckBrickCollisions();

        // Victory check
        bool anyBrick = false;
        for (int r = 0; r < BrickRows && !anyBrick; r++)
            for (int c = 0; c < BrickCols && !anyBrick; c++)
                if (_bricks[r, c]) anyBrick = true;

        if (!anyBrick)
            Phase = GamePhase.Victory;
    }

    private void CheckBrickCollisions()
    {
        for (int r = 0; r < BrickRows; r++)
        {
            for (int c = 0; c < BrickCols; c++)
            {
                if (!_bricks[r, c])
                    continue;

                float bx = BricksStartX + c * (BrickWidth + BrickGap);
                float by = BricksStartY + r * (BrickHeight + BrickGap);

                if (_ballX + BallRadius < bx || _ballX - BallRadius > bx + BrickWidth ||
                    _ballY + BallRadius < by || _ballY - BallRadius > by + BrickHeight)
                    continue;

                _bricks[r, c] = false;
                Score += 10 * (BrickRows - r); // top rows worth more

                // Pick bounce axis with smallest overlap
                float overlapLeft   = (_ballX + BallRadius) - bx;
                float overlapRight  = (bx + BrickWidth) - (_ballX - BallRadius);
                float overlapTop    = (_ballY + BallRadius) - by;
                float overlapBottom = (by + BrickHeight) - (_ballY - BallRadius);
                float minH = MathF.Min(overlapLeft, overlapRight);
                float minV = MathF.Min(overlapTop, overlapBottom);

                if (minV <= minH)
                    _ballVY = -_ballVY;
                else
                    _ballVX = -_ballVX;

                return; // only break one brick per frame
            }
        }
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        // Scale game coordinates → canvas coordinates, maintaining aspect ratio
        float scale = MathF.Min(width / (float)GameWidth, height / (float)GameHeight);
        float offsetX = (width  - GameWidth  * scale) / 2f;
        float offsetY = (height - GameHeight * scale) / 2f;

        canvas.Save();
        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale, scale);

        switch (Phase)
        {
            case GamePhase.Start:    DrawStartScreen(canvas);   break;
            case GamePhase.Playing:  DrawGameScreen(canvas);    break;
            case GamePhase.GameOver: DrawGameOverScreen(canvas); break;
            case GamePhase.Victory:  DrawVictoryScreen(canvas);  break;
        }

        canvas.Restore();
    }

    // ── Screen Renderers ───────────────────────────────────────────────────

    private void DrawStartScreen(SKCanvas canvas)
    {
        // Decorative faded bricks
        for (int r = 0; r < BrickRows; r++)
            for (int c = 0; c < BrickCols; c++)
                DrawBrick(canvas,
                    BricksStartX + c * (BrickWidth + BrickGap),
                    BricksStartY + r * (BrickHeight + BrickGap),
                    BrickColors[r], 0.25f);

        DrawCenteredText(canvas, "BREAKOUT",      72f, SKColors.White,  290f);
        DrawCenteredText(canvas, "Click or Tap to Start", 28f, AccentColor, 360f);
        DrawCenteredText(canvas, "Move mouse / finger to control the paddle", 18f, DimColor, 415f);
    }

    private void DrawGameScreen(SKCanvas canvas)
    {
        DrawBricks(canvas);
        DrawPaddle(canvas);
        DrawBall(canvas);
        DrawHUD(canvas);
    }

    private void DrawGameOverScreen(SKCanvas canvas)
    {
        DrawGameScreen(canvas);
        DrawOverlay(canvas);
        DrawCenteredText(canvas, "GAME OVER", 64f, new SKColor(0xFF, 0x2D, 0x55), 270f);
        DrawCenteredText(canvas, $"Score: {Score}", 32f, SKColors.White, 335f);
        DrawCenteredText(canvas, "Click or Tap to Play Again", 24f, AccentColor, 395f);
    }

    private void DrawVictoryScreen(SKCanvas canvas)
    {
        DrawGameScreen(canvas);
        DrawOverlay(canvas);
        DrawCenteredText(canvas, "YOU WIN!",    64f, new SKColor(0xFF, 0xD6, 0x0A), 270f);
        DrawCenteredText(canvas, $"Final Score: {Score}", 32f, SKColors.White, 335f);
        DrawCenteredText(canvas, "Click or Tap to Play Again", 24f, AccentColor, 395f);
    }

    // ── Drawing Helpers ────────────────────────────────────────────────────

    private void DrawBricks(SKCanvas canvas)
    {
        for (int r = 0; r < BrickRows; r++)
            for (int c = 0; c < BrickCols; c++)
                if (_bricks[r, c])
                    DrawBrick(canvas,
                        BricksStartX + c * (BrickWidth + BrickGap),
                        BricksStartY + r * (BrickHeight + BrickGap),
                        BrickColors[r], 1f);
    }

    private static void DrawBrick(SKCanvas canvas, float x, float y, SKColor color, float opacity)
    {
        byte a = (byte)(255 * opacity);
        var rect = SKRect.Create(x, y, BrickWidth, BrickHeight);

        using var fill = new SKPaint { Color = color.WithAlpha(a), IsAntialias = true };
        canvas.DrawRoundRect(rect, 3f, 3f, fill);

        using var shine = new SKPaint { Color = SKColors.White.WithAlpha((byte)(55 * opacity)), IsAntialias = true };
        canvas.DrawRoundRect(SKRect.Create(x + 2f, y + 2f, BrickWidth - 4f, (BrickHeight - 4f) / 2f), 2f, 2f, shine);
    }

    private void DrawPaddle(SKCanvas canvas)
    {
        using var paint = new SKPaint { Color = PaddleColor, IsAntialias = true };
        canvas.DrawRoundRect(SKRect.Create(_paddleX, PaddleY, PaddleWidth, PaddleHeight), 6f, 6f, paint);
    }

    private void DrawBall(SKCanvas canvas)
    {
        using var glow = new SKPaint { Color = SKColors.White.WithAlpha(60), IsAntialias = true, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f) };
        canvas.DrawCircle(_ballX, _ballY, BallRadius + 2f, glow);

        using var ball = new SKPaint { Color = SKColors.White, IsAntialias = true };
        canvas.DrawCircle(_ballX, _ballY, BallRadius, ball);
    }

    private void DrawHUD(SKCanvas canvas)
    {
        using var font  = new SKFont(SKTypeface.Default, 20f);
        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };

        canvas.DrawText($"Score: {Score}", 20f, 30f, font, paint);

        string livesText = $"Lives: {Lives}";
        float livesW = font.MeasureText(livesText);
        canvas.DrawText(livesText, GameWidth - livesW - 20f, 30f, font, paint);
    }

    private static void DrawOverlay(SKCanvas canvas)
    {
        using var overlay = new SKPaint { Color = new SKColor(0, 0, 0, 185) };
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), overlay);
    }

    private static void DrawCenteredText(SKCanvas canvas, string text, float size, SKColor color, float y)
    {
        using var font  = new SKFont(SKTypeface.Default, size) { Edging = SKFontEdging.Antialias };
        using var paint = new SKPaint { Color = color, IsAntialias = true };
        float w = font.MeasureText(text);
        canvas.DrawText(text, (GameWidth - w) / 2f, y, font, paint);
    }

}
