using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.Breakout.BreakoutConstants;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

/// <summary>Active gameplay screen: ball, paddle, bricks, power-ups, HUD.</summary>
internal sealed class BreakoutPlayScreen(BreakoutGameState state) : GameScreen
{
    // ── Physics ───────────────────────────────────────────────────────────
    private float _paddleX;
    private readonly CircleBody _ball = new() { Radius = BallRadius };
    private readonly RectBody   _paddleBody = new() { Height = PaddleHeight, IsStatic = true };

    // ── Animated paddle width ─────────────────────────────────────────────
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

    private readonly List<Brick>          _bricks   = [];
    private readonly List<FallingPowerUp> _powerUps = [];

    public override void OnActivated()
    {
        state.Score = 0;
        state.Lives = 3;
        _strongBallTimer = 0f;
        _bigPaddleTimer  = 0f;
        _powerUps.Clear();
        InitBricks();
        _paddleX = (GameWidth - DefaultPaddleWidth) / 2f;
        _paddleWidth.SetImmediate(DefaultPaddleWidth);
        ResetBall();
    }

    // ── Initialisation ────────────────────────────────────────────────────

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
        _ball.VelocityX =  (float)(BallSpeed * Math.Cos(angle));
        _ball.VelocityY = -(float)(BallSpeed * Math.Sin(angle));
    }

    // ── Input ─────────────────────────────────────────────────────────────

    public override void OnPointerMove(float x, float y)
    {
        float pw = CurrentPaddleWidth;
        _paddleX = Math.Clamp(x - pw / 2f, 0f, GameWidth - pw);
    }

    // ── Update ────────────────────────────────────────────────────────────

    public override void Update(float deltaTime)
    {
        _paddleWidth.Update(deltaTime);

        foreach (var brick in _bricks)
            brick.Sprite.Update(deltaTime);

        // Power-up timers
        _strongBallTimer = Math.Max(0f, _strongBallTimer - deltaTime);

        float prevBigPaddleTimer = _bigPaddleTimer;
        _bigPaddleTimer = Math.Max(0f, _bigPaddleTimer - deltaTime);
        if (prevBigPaddleTimer > 0f && _bigPaddleTimer <= 0f)
        {
            _paddleWidth.AnimateTo(DefaultPaddleWidth, 0.4f, Easing.EaseIn);
            _paddleX = Math.Clamp(_paddleX, 0f, GameWidth - DefaultPaddleWidth);
        }

        _ball.Step(deltaTime);

        // Wall collisions
        if      (_ball.X - BallRadius < 0f)       { _ball.X = BallRadius;             _ball.VelocityX =  MathF.Abs(_ball.VelocityX); }
        else if (_ball.X + BallRadius > GameWidth) { _ball.X = GameWidth - BallRadius; _ball.VelocityX = -MathF.Abs(_ball.VelocityX); }
        if      (_ball.Y - BallRadius < 0f)        { _ball.Y = BallRadius;             _ball.VelocityY =  MathF.Abs(_ball.VelocityY); }

        // Ball lost
        if (_ball.Y > GameHeight + BallRadius * 2)
        {
            state.Lives--;
            if (state.Lives <= 0)
                Game?.PushOverlay<BreakoutGameOverScreen>();
            else
                ResetBall();
            return;
        }

        // Paddle collision
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

        if (!_bricks.Any(b => b.Active))
            Game?.PushOverlay<BreakoutVictoryScreen>();
    }

    private void CheckBrickCollisions()
    {
        bool piercing = _strongBallTimer > 0f;

        foreach (var brick in _bricks)
        {
            if (!brick.Active || !_ball.Overlaps(brick.Body)) continue;

            brick.Active = false;
            state.Score += 10 * (BrickRows - brick.Row);
            TrySpawnPowerUp(brick.Body.X + BrickWidth / 2f, brick.Body.Y + BrickHeight / 2f);

            if (!piercing)
            {
                _ball.Reflect(brick.Body);
                return;
            }
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
                _paddleWidth.AnimateTo(DefaultPaddleWidth * BigPaddleMultiplier, 0.3f, Easing.BackOut);
                break;
        }
    }

    // ── Draw ──────────────────────────────────────────────────────────────

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);
        DrawGameContent(canvas);
    }

    internal void DrawGameContent(SKCanvas canvas)
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

        // Paddle
        float pw = CurrentPaddleWidth;
        _paddleSprite.X     = _paddleX;
        _paddleSprite.Y     = PaddleY;
        _paddleSprite.Width = pw;
        _paddleSprite.Color = _bigPaddleTimer > 0f || _paddleWidth.IsAnimating
            ? BigPaddleColor : PaddleColor;
        _paddleSprite.Draw(canvas);

        // Ball
        _ballSprite.X         = _ball.X;
        _ballSprite.Y         = _ball.Y;
        _ballSprite.Color     = _strongBallTimer > 0f ? StrongBallColor : SKColors.White;
        _ballSprite.GlowColor = _strongBallTimer > 0f ? StrongBallColor : SKColors.White;
        _ballSprite.Draw(canvas);

        // HUD
        DrawHelper.DrawText(canvas, $"Score: {state.Score}", 20f, SKColors.White, 20f, 30f);
        string livesText = $"Lives: {state.Lives}";
        float  livesW    = DrawHelper.MeasureText(livesText, 20f);
        DrawHelper.DrawText(canvas, livesText, 20f, SKColors.White, GameWidth - livesW - 20f, 30f);

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
