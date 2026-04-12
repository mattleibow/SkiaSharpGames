using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

public enum GamePhase { Start, Playing, GameOver, Victory }
public enum PowerUpType { StrongBall, BigPaddle }

public class BreakoutGameEngine : GameScreenBase
{
    // ── Game dimensions ───────────────────────────────────────────────────
    public const int GameWidth  = 800;
    public const int GameHeight = 600;

    // ── Paddle ────────────────────────────────────────────────────────────
    private const float DefaultPaddleWidth  = 100f;
    private const float PaddleHeight        = 14f;
    private const float PaddleY             = 558f;
    private const float BigPaddleMultiplier = 1.8f;
    private const float BigPaddleDuration   = 8f;

    // ── Ball ──────────────────────────────────────────────────────────────
    private const float BallRadius         = 8f;
    private const float BallSpeed          = 350f;
    private const float StrongBallDuration = 5f;

    // ── Bricks ────────────────────────────────────────────────────────────
    private const int   BrickCols   = 10;
    private const int   BrickRows   = 5;
    private const float BrickWidth  = 72f;
    private const float BrickHeight = 22f;
    private const float BrickGap    = 4f;
    private static readonly float BricksStartX =
        (GameWidth - (BrickCols * (BrickWidth + BrickGap) - BrickGap)) / 2f;
    private const float BricksStartY = 60f;

    // ── Power-ups ─────────────────────────────────────────────────────────
    private const float PowerUpChance = 0.15f;
    private const float PowerUpSpeed  = 130f;
    private const float PowerUpW      = 34f;
    private const float PowerUpH      = 18f;

    // ── Colours ───────────────────────────────────────────────────────────
    private static readonly SKColor BackgroundColor = new(0x0D, 0x1B, 0x2A);
    private static readonly SKColor PaddleColor     = new(0x00, 0xD4, 0xFF);
    private static readonly SKColor AccentColor     = new(0x00, 0xD4, 0xFF);
    private static readonly SKColor DimColor        = new(0xAA, 0xAA, 0xAA);
    private static readonly SKColor StrongBallColor = new(0xFF, 0x6B, 0x00);
    private static readonly SKColor BigPaddleColor  = new(0x00, 0xE5, 0x76);

    private static readonly SKColor[] BrickColors =
    [
        new SKColor(0xFF, 0x2D, 0x55), // Red    (row 0 – top, highest score)
        new SKColor(0xFF, 0x9F, 0x0A), // Orange
        new SKColor(0xFF, 0xD6, 0x0A), // Yellow
        new SKColor(0x30, 0xD1, 0x58), // Green
        new SKColor(0x0A, 0x84, 0xFF), // Blue   (row 4 – bottom)
    ];

    public override (int width, int height) GameDimensions => (GameWidth, GameHeight);

    // ── Public state ──────────────────────────────────────────────────────
    public GamePhase Phase { get; private set; } = GamePhase.Start;
    public int       Score { get; private set; }
    public int       Lives { get; private set; } = 3;

    // ── Physics ───────────────────────────────────────────────────────────
    private float _paddleX;

    // Ball as a PhysicsBody (circle) — position, velocity, and collision all in one place
    private readonly PhysicsBody _ball = new(PhysicsShape.Circle) { Radius = BallRadius };

    // Paddle physics body — updated each frame before collision checks
    private readonly PhysicsBody _paddleBody = new(PhysicsShape.Rect)
        { Height = PaddleHeight, IsStatic = true };

    // ── Animated paddle width ─────────────────────────────────────────────
    // Smoothly transitions between DefaultPaddleWidth and BigPaddle size using BackOut/EaseIn.
    private readonly AnimatedFloat _paddleWidth = new(DefaultPaddleWidth);
    private float CurrentPaddleWidth => _paddleWidth.Value;

    // ── Power-up timers ───────────────────────────────────────────────────
    private float _strongBallTimer;
    private float _bigPaddleTimer;

    // ── Sprites ───────────────────────────────────────────────────────────
    private readonly CircleSprite _ballSprite = new()
    {
        Radius = BallRadius, Color = SKColors.White, GlowRadius = 4f, GlowColor = SKColors.White
    };

    private readonly RectSprite _paddleSprite = new()
    {
        Height = PaddleHeight, Color = PaddleColor, CornerRadius = 6f, ShowShine = false
    };

    // ── Nested data types ─────────────────────────────────────────────────

    private sealed class Brick
    {
        public readonly int Row, Col;
        public bool Active = true;
        public readonly RectSprite  Sprite;
        public readonly PhysicsBody Body;

        public Brick(int row, int col, float x, float y)
        {
            Row = row; Col = col;
            Sprite = new RectSprite
            {
                X = x, Y = y, Width = BrickWidth, Height = BrickHeight,
                CornerRadius = 3f, ShowShine = true
            };
            Body = new PhysicsBody(PhysicsShape.Rect)
            {
                X = x, Y = y, Width = BrickWidth, Height = BrickHeight, IsStatic = true
            };
        }
    }

    private sealed class FallingPowerUp
    {
        public float X, Y;   // centre
        public PowerUpType Type;
        public readonly RectSprite Sprite = new()
        {
            Width = PowerUpW, Height = PowerUpH, CornerRadius = 5f, ShowShine = false
        };
    }

    private readonly List<Brick>          _bricks  = [];
    private readonly List<FallingPowerUp> _powerUps = [];

    public BreakoutGameEngine()
    {
        // Pre-populate bricks so the start screen can show the decorative grid
        InitBricks();
    }

    // ── Initialisation ────────────────────────────────────────────────────

    public void StartGame()
    {
        Phase           = GamePhase.Playing;
        Score           = 0;
        Lives           = 3;
        _strongBallTimer = 0f;
        _bigPaddleTimer  = 0f;
        _powerUps.Clear();
        InitBricks();
        _paddleX = (GameWidth - DefaultPaddleWidth) / 2f;
        _paddleWidth.SetImmediate(DefaultPaddleWidth);
        ResetBall();
    }

    private void InitBricks()
    {
        _bricks.Clear();
        for (int r = 0; r < BrickRows; r++)
        {
            for (int c = 0; c < BrickCols; c++)
            {
                float bx = BricksStartX + c * (BrickWidth + BrickGap);
                float by = BricksStartY + r * (BrickHeight + BrickGap);
                var brick = new Brick(r, c, bx, by);
                brick.Sprite.Color = BrickColors[r];
                // Stagger shimmer so bricks don't all flash at once
                brick.Sprite.Shimmer.Start(Random.Shared.NextSingle() * brick.Sprite.Shimmer.Period);
                _bricks.Add(brick);
            }
        }
    }

    private void ResetBall()
    {
        _ball.X = _paddleX + CurrentPaddleWidth / 2f;
        _ball.Y = PaddleY - BallRadius - 10f;
        double angle = (35.0 + Random.Shared.NextDouble() * 110.0) * Math.PI / 180.0;
        _ball.VelocityX = (float)(BallSpeed * Math.Cos(angle));
        _ball.VelocityY = -(float)(BallSpeed * Math.Sin(angle));
    }

    // ── Input ─────────────────────────────────────────────────────────────

    public override void OnPointerMove(float x, float y)
    {
        if (Phase == GamePhase.Playing)
        {
            float pw = CurrentPaddleWidth;
            _paddleX = Math.Clamp(x - pw / 2f, 0f, GameWidth - pw);
        }
    }

    public override void OnPointerDown(float x, float y)
    {
        if (IsTransitioning) return;
        if (Phase is GamePhase.Start or GamePhase.GameOver or GamePhase.Victory)
            BeginTransition(new FadeTransition(), 0.35f, StartGame);
    }

    // ── Update ────────────────────────────────────────────────────────────

    public override void Update(float deltaTime)
    {
        UpdateTransition(deltaTime);

        // Always advance brick shimmer (visible on start screen too)
        foreach (var brick in _bricks)
            brick.Sprite.Update(deltaTime);

        if (Phase != GamePhase.Playing) return;

        // Advance animated paddle width
        _paddleWidth.Update(deltaTime);

        // Power-up timers
        _strongBallTimer = Math.Max(0f, _strongBallTimer - deltaTime);

        float prevBigPaddleTimer = _bigPaddleTimer;
        _bigPaddleTimer = Math.Max(0f, _bigPaddleTimer - deltaTime);
        if (prevBigPaddleTimer > 0f && _bigPaddleTimer <= 0f)
        {
            _paddleWidth.AnimateTo(DefaultPaddleWidth, 0.4f, Easing.EaseIn);
            _paddleX = Math.Clamp(_paddleX, 0f, GameWidth - DefaultPaddleWidth);
        }

        // Step ball using PhysicsBody
        _ball.Step(deltaTime);

        // Wall collisions
        if      (_ball.X - BallRadius < 0f)       { _ball.X = BallRadius;             _ball.VelocityX =  MathF.Abs(_ball.VelocityX); }
        else if (_ball.X + BallRadius > GameWidth) { _ball.X = GameWidth - BallRadius; _ball.VelocityX = -MathF.Abs(_ball.VelocityX); }
        if      (_ball.Y - BallRadius < 0f)        { _ball.Y = BallRadius;             _ball.VelocityY =  MathF.Abs(_ball.VelocityY); }

        // Ball lost
        if (_ball.Y > GameHeight + BallRadius * 2)
        {
            Lives--;
            if (Lives <= 0 && !IsTransitioning)
                BeginTransition(new FadeTransition(), 0.4f, () => Phase = GamePhase.GameOver);
            else if (Lives > 0)
                ResetBall();
            return;
        }

        // Paddle collision — use PhysicsBody for bounds, keep angled-bounce logic
        float pw = CurrentPaddleWidth;
        _paddleBody.X     = _paddleX;
        _paddleBody.Y     = PaddleY;
        _paddleBody.Width = pw;
        if (_ball.VelocityY > 0f && _ball.Overlaps(_paddleBody))
        {
            float hitPos = (_ball.X - (_paddleX + pw / 2f)) / (pw / 2f);
            float angle  = hitPos * (65f * MathF.PI / 180f);
            _ball.VelocityX = BallSpeed * MathF.Sin(angle);
            _ball.VelocityY = -BallSpeed * MathF.Cos(angle);
        }

        CheckBrickCollisions();
        UpdateFallingPowerUps(deltaTime);

        // Victory
        if (!IsTransitioning && !_bricks.Any(b => b.Active))
            BeginTransition(new FadeTransition(), 0.4f, () => Phase = GamePhase.Victory);
    }

    private void CheckBrickCollisions()
    {
        bool piercing = _strongBallTimer > 0f;

        foreach (var brick in _bricks)
        {
            if (!brick.Active || !_ball.Overlaps(brick.Body)) continue;

            brick.Active = false;
            Score += 10 * (BrickRows - brick.Row);
            TrySpawnPowerUp(brick.Body.X + BrickWidth / 2f, brick.Body.Y + BrickHeight / 2f);

            if (!piercing)
            {
                _ball.Reflect(brick.Body);
                return; // one brick per frame in normal mode
            }
            // Piercing: mark brick as hit but don't reflect — keep going
        }
    }

    private void TrySpawnPowerUp(float cx, float cy)
    {
        if (Random.Shared.NextDouble() >= PowerUpChance) return;
        var type = Random.Shared.NextDouble() < 0.5 ? PowerUpType.StrongBall : PowerUpType.BigPaddle;
        var pu = new FallingPowerUp { X = cx, Y = cy, Type = type };
        pu.Sprite.X     = cx - PowerUpW / 2f;
        pu.Sprite.Y     = cy - PowerUpH / 2f;
        pu.Sprite.Color = type == PowerUpType.StrongBall ? StrongBallColor : BigPaddleColor;
        _powerUps.Add(pu);
    }

    private void UpdateFallingPowerUps(float deltaTime)
    {
        float pw = CurrentPaddleWidth;

        for (int i = _powerUps.Count - 1; i >= 0; i--)
        {
            var pu = _powerUps[i];

            pu.Y       += PowerUpSpeed * deltaTime;
            pu.Sprite.Y = pu.Y - PowerUpH / 2f;

            if (pu.Y + PowerUpH / 2f >= PaddleY &&
                pu.Y - PowerUpH / 2f <= PaddleY + PaddleHeight &&
                pu.X >= _paddleX && pu.X <= _paddleX + pw)
            {
                ApplyPowerUp(pu.Type);
                _powerUps.RemoveAt(i);
            }
            else if (pu.Y > GameHeight + 30f)
            {
                _powerUps.RemoveAt(i);
            }
        }
    }

    private void ApplyPowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.StrongBall:
                _strongBallTimer = StrongBallDuration;
                break;
            case PowerUpType.BigPaddle:
                _bigPaddleTimer = BigPaddleDuration;
                // Animate paddle expansion with a springy BackOut overshoot
                _paddleWidth.AnimateTo(DefaultPaddleWidth * BigPaddleMultiplier, 0.3f, Easing.BackOut);
                break;
        }
    }

    // ── Draw ──────────────────────────────────────────────────────────────

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        float scale   = MathF.Min(width / (float)GameWidth, height / (float)GameHeight);
        float offsetX = (width  - GameWidth  * scale) / 2f;
        float offsetY = (height - GameHeight * scale) / 2f;

        canvas.Save();
        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale, scale);

        switch (Phase)
        {
            case GamePhase.Start:   DrawStartScreen(canvas); break;
            case GamePhase.Playing: DrawGameScreen(canvas);  break;
            case GamePhase.GameOver:
                DrawGameScreen(canvas);
                DrawHelper.DrawOverlay(canvas, GameWidth, GameHeight);
                DrawHelper.DrawCenteredText(canvas, "GAME OVER", 64f, new SKColor(0xFF, 0x2D, 0x55), GameWidth / 2f, 270f);
                DrawHelper.DrawCenteredText(canvas, $"Score: {Score}", 32f, SKColors.White, GameWidth / 2f, 335f);
                DrawHelper.DrawCenteredText(canvas, "Click or Tap to Play Again", 24f, AccentColor, GameWidth / 2f, 395f);
                break;
            case GamePhase.Victory:
                DrawGameScreen(canvas);
                DrawHelper.DrawOverlay(canvas, GameWidth, GameHeight);
                DrawHelper.DrawCenteredText(canvas, "YOU WIN!", 64f, new SKColor(0xFF, 0xD6, 0x0A), GameWidth / 2f, 270f);
                DrawHelper.DrawCenteredText(canvas, $"Final Score: {Score}", 32f, SKColors.White, GameWidth / 2f, 335f);
                DrawHelper.DrawCenteredText(canvas, "Click or Tap to Play Again", 24f, AccentColor, GameWidth / 2f, 395f);
                break;
        }

        DrawTransitionOverlay(canvas);
        canvas.Restore();
    }

    private void DrawStartScreen(SKCanvas canvas)
    {
        foreach (var brick in _bricks)
        {
            brick.Sprite.Alpha = 0.3f;
            brick.Sprite.Draw(canvas);
        }
        DrawHelper.DrawCenteredText(canvas, "BREAKOUT", 72f, SKColors.White, GameWidth / 2f, 290f);
        DrawHelper.DrawCenteredText(canvas, "Click or Tap to Start", 28f, AccentColor, GameWidth / 2f, 360f);
        DrawHelper.DrawCenteredText(canvas, "Move mouse / finger to control the paddle", 18f, DimColor, GameWidth / 2f, 415f);
    }

    private void DrawGameScreen(SKCanvas canvas)
    {
        // Bricks
        foreach (var brick in _bricks)
        {
            if (!brick.Active) continue;
            brick.Sprite.Alpha = 1f;
            brick.Sprite.Draw(canvas);
        }

        // Falling power-ups
        foreach (var pu in _powerUps)
        {
            pu.Sprite.Draw(canvas);
            string label = pu.Type == PowerUpType.StrongBall ? "S" : "B";
            DrawHelper.DrawCenteredText(canvas, label, 11f, SKColors.White, pu.X, pu.Y + 4f);
        }

        // Paddle — width from AnimatedFloat, colour reflects active powerup
        float pw = CurrentPaddleWidth;
        _paddleSprite.X     = _paddleX;
        _paddleSprite.Y     = PaddleY;
        _paddleSprite.Width = pw;
        _paddleSprite.Color = _bigPaddleTimer > 0f || _paddleWidth.IsAnimating
            ? BigPaddleColor : PaddleColor;
        _paddleSprite.Draw(canvas);

        // Ball — colour reflects StrongBall powerup
        _ballSprite.X         = _ball.X;
        _ballSprite.Y         = _ball.Y;
        _ballSprite.Color     = _strongBallTimer > 0f ? StrongBallColor : SKColors.White;
        _ballSprite.GlowColor = _strongBallTimer > 0f ? StrongBallColor : SKColors.White;
        _ballSprite.Draw(canvas);

        // HUD
        DrawHelper.DrawText(canvas, $"Score: {Score}", 20f, SKColors.White, 20f, 30f);
        string livesText = $"Lives: {Lives}";
        float  livesW    = DrawHelper.MeasureText(livesText, 20f);
        DrawHelper.DrawText(canvas, livesText, 20f, SKColors.White, GameWidth - livesW - 20f, 30f);

        // Active power-up timers
        float hudY = 52f;
        if (_strongBallTimer > 0f)
        {
            DrawHelper.DrawCenteredText(canvas, $"STRONG BALL {_strongBallTimer:F1}s",
                14f, StrongBallColor, GameWidth / 2f, hudY);
            hudY += 18f;
        }
        if (_bigPaddleTimer > 0f)
        {
            DrawHelper.DrawCenteredText(canvas, $"BIG PADDLE {_bigPaddleTimer:F1}s",
                14f, BigPaddleColor, GameWidth / 2f, hudY);
        }
    }
}
