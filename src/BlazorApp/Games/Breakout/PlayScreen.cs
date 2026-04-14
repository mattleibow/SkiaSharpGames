using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.Breakout.BreakoutConstants;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

/// <summary>Active gameplay screen: ball, paddle, bricks, power-ups, HUD.</summary>
internal sealed class PlayScreen(BreakoutGameState state, IScreenCoordinator coordinator) : GameScreen
{
    // ── Entities ──────────────────────────────────────────────────────────
    private readonly Ball _ball = new();
    private readonly Paddle _paddle = new();

    // ── Animated paddle width ─────────────────────────────────────────────
    private readonly AnimatedFloat _paddleWidth = new(DefaultPaddleWidth);
    private float CurrentPaddleWidth => _paddleWidth.Value;

    // ── Power-up timers ───────────────────────────────────────────────────
    private float _strongBallTimer;
    private float _bigPaddleTimer;

    private readonly List<Brick> _bricks = [];
    private readonly List<FallingPowerUp> _powerUps = [];

    public override void OnActivated()
    {
        state.Score = 0;
        state.Lives = 3;
        _strongBallTimer = 0f;
        _bigPaddleTimer = 0f;
        _powerUps.Clear();
        InitBricks();
        _paddle.X = GameWidth / 2f;
        _paddle.Y = PaddleY + PaddleHeight / 2f;
        _paddleWidth.SetImmediate(DefaultPaddleWidth);
        _paddle.Collider.Width = DefaultPaddleWidth;
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
                float cx = BricksStartX + c * (BrickWidth + BrickGap) + BrickWidth / 2f;
                float cy = BricksStartY + r * (BrickHeight + BrickGap) + BrickHeight / 2f;
                var brick = new Brick(r, c, cx, cy);
                brick.Sprite.Color = BrickColors[r];
                brick.Sprite.Shimmer.Start(Random.Shared.NextSingle() * brick.Sprite.Shimmer.Period);
                _bricks.Add(brick);
            }
        }
    }

    private void ResetBall()
    {
        _ball.X = _paddle.X;
        _ball.Y = PaddleY - BallRadius - 10f;
        double angle = (35.0 + Random.Shared.NextDouble() * 110.0) * Math.PI / 180.0;
        _ball.Rigidbody.VelocityX = (float)(BallSpeed * Math.Cos(angle));
        _ball.Rigidbody.VelocityY = -(float)(BallSpeed * Math.Sin(angle));
    }

    // ── Input ─────────────────────────────────────────────────────────────

    public override void OnPointerMove(float x, float y)
    {
        float pw = CurrentPaddleWidth;
        _paddle.X = Math.Clamp(x, pw / 2f, GameWidth - pw / 2f);
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
            float halfW = DefaultPaddleWidth / 2f;
            _paddle.X = Math.Clamp(_paddle.X, halfW, GameWidth - halfW);
        }

        // Update paddle collider width to match the animated value
        float pw = CurrentPaddleWidth;
        _paddle.Collider.Width = pw;

        // Physics step
        _ball.Rigidbody.Step(_ball, deltaTime);

        // Wall collisions
        ResolveWallCollisions();

        // Ball lost
        if (_ball.Y > GameHeight + BallRadius * 2)
        {
            state.Lives--;
            if (state.Lives <= 0)
                coordinator.PushOverlay<GameOverScreen>();
            else
                ResetBall();
            return;
        }

        // Paddle collision
        ResolvePaddleCollision();

        // Brick collisions
        CheckBrickCollisions();

        // Falling power-ups
        UpdateFallingPowerUps(deltaTime);

        if (!_bricks.Any(b => b.Active))
            coordinator.PushOverlay<VictoryScreen>();
    }

    private void ResolveWallCollisions()
    {
        if (_ball.X - BallRadius < 0f)
        {
            _ball.X = BallRadius;
            _ball.Rigidbody.VelocityX = MathF.Abs(_ball.Rigidbody.VelocityX);
        }
        else if (_ball.X + BallRadius > GameWidth)
        {
            _ball.X = GameWidth - BallRadius;
            _ball.Rigidbody.VelocityX = -MathF.Abs(_ball.Rigidbody.VelocityX);
        }

        if (_ball.Y - BallRadius < 0f)
        {
            _ball.Y = BallRadius;
            _ball.Rigidbody.VelocityY = MathF.Abs(_ball.Rigidbody.VelocityY);
        }
    }

    private void ResolvePaddleCollision()
    {
        if (_ball.Rigidbody.VelocityY > 0f &&
            CollisionResolver.Overlaps(_ball, _ball.Collider, _paddle, _paddle.Collider))
        {
            float pw = CurrentPaddleWidth;
            float hitPos = (_ball.X - _paddle.X) / (pw / 2f);
            float angle = hitPos * (65f * MathF.PI / 180f);
            _ball.Rigidbody.VelocityX = BallSpeed * MathF.Sin(angle);
            _ball.Rigidbody.VelocityY = -BallSpeed * MathF.Cos(angle);
        }
    }

    private void CheckBrickCollisions()
    {
        bool piercing = _strongBallTimer > 0f;

        foreach (var brick in _bricks)
        {
            if (!brick.Active || !CollisionResolver.Overlaps(_ball, _ball.Collider, brick, brick.Collider)) continue;

            brick.Active = false;
            state.Score += 10 * (BrickRows - brick.Row);
            TrySpawnPowerUp(brick.X, brick.Y);

            if (!piercing)
            {
                CollisionResolver.Reflect(_ball, _ball.Collider, _ball.Rigidbody, brick, brick.Collider);
                return;
            }
        }
    }

    private void TrySpawnPowerUp(float cx, float cy)
    {
        if (Random.Shared.NextDouble() >= PowerUpChance) return;
        var type = Random.Shared.NextDouble() < 0.5 ? PowerUpType.StrongBall : PowerUpType.BigPaddle;
        var pu = new FallingPowerUp { X = cx, Y = cy, Type = type };
        pu.Sprite.Color = type == PowerUpType.StrongBall ? StrongBallColor : BigPaddleColor;
        _powerUps.Add(pu);
    }

    private void UpdateFallingPowerUps(float deltaTime)
    {
        float pw = CurrentPaddleWidth;

        for (int i = _powerUps.Count - 1; i >= 0; i--)
        {
            var pu = _powerUps[i];

            pu.Rigidbody.Step(pu, deltaTime);

            // Paddle collision: power-up centre Y within paddle vertical range, X within paddle width
            if (pu.Y + PowerUpH / 2f >= PaddleY &&
                pu.Y - PowerUpH / 2f <= PaddleY + PaddleHeight &&
                pu.X >= _paddle.X - pw / 2f && pu.X <= _paddle.X + pw / 2f)
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
            brick.Sprite.Draw(canvas, brick.X, brick.Y);
        }

        // Falling power-ups
        foreach (var pu in _powerUps)
        {
            pu.Sprite.Draw(canvas, pu.X, pu.Y);
            string label = pu.Type == PowerUpType.StrongBall ? "S" : "B";
            DrawHelper.DrawCenteredText(canvas, label, 11f, SKColors.White, pu.X, pu.Y + 4f);
        }

        // Paddle
        float pw = CurrentPaddleWidth;
        _paddle.Sprite.Width = pw;
        _paddle.Sprite.Color = _bigPaddleTimer > 0f || _paddleWidth.IsAnimating
            ? BigPaddleColor : PaddleColor;
        _paddle.Sprite.Draw(canvas, _paddle.X, _paddle.Y);

        // Ball
        _ball.Sprite.Color = _strongBallTimer > 0f ? StrongBallColor : SKColors.White;
        _ball.Sprite.GlowColor = _strongBallTimer > 0f ? StrongBallColor : SKColors.White;
        _ball.Sprite.Draw(canvas, _ball.X, _ball.Y);

        // HUD
        DrawHelper.DrawText(canvas, $"Score: {state.Score}", 20f, SKColors.White, 20f, 30f);
        string livesText = $"Lives: {state.Lives}";
        float livesW = DrawHelper.MeasureText(livesText, 20f);
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
