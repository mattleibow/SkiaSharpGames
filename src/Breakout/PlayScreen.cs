using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>Active gameplay screen: ball, paddle, bricks, power-ups, HUD.</summary>
internal sealed class PlayScreen(BreakoutGameState state, IScreenCoordinator coordinator) : GameScreen
{
    // ── Entities ──────────────────────────────────────────────────────────
    private readonly Ball _ball = new();
    private readonly Paddle _paddle = new();

    // ── Boundary walls ────────────────────────────────────────────────────
    private static readonly float WallThickness = 100f;
    private readonly Wall _leftWall   = new(-WallThickness / 2f, GameHeight / 2f, WallThickness, GameHeight + WallThickness * 2);
    private readonly Wall _topWall    = new(GameWidth / 2f, -WallThickness / 2f, GameWidth + WallThickness * 2, WallThickness);
    private readonly Wall _rightWall  = new(GameWidth + WallThickness / 2f, GameHeight / 2f, WallThickness, GameHeight + WallThickness * 2);
    private readonly Wall _bottomWall = new(GameWidth / 2f, GameHeight + WallThickness / 2f, GameWidth + WallThickness * 2, WallThickness);

    // ── Power-up timers ───────────────────────────────────────────────────
    private CountdownTimer _strongBallTimer;
    private CountdownTimer _bigPaddleTimer;

    private readonly List<Brick> _bricks = [];
    private readonly List<FallingPowerUp> _powerUps = [];

    // ── Text sprites ──────────────────────────────────────────────────────
    private readonly TextSprite _scoreText = new() { Size = 20f, Color = SKColors.White };
    private readonly TextSprite _livesText = new() { Size = 20f, Color = SKColors.White, Align = TextAlign.Right };
    private readonly TextSprite _powerUpLabel = new() { Size = 11f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly TextSprite _strongBallText = new() { Size = 14f, Align = TextAlign.Center };
    private readonly TextSprite _bigPaddleText = new() { Size = 14f, Align = TextAlign.Center };

    public override void OnActivated()
    {
        state.Score = 0;
        state.Lives = 3;
        _strongBallTimer = default;
        _bigPaddleTimer = default;
        _powerUps.Clear();
        InitBricks();
        _paddle.X = GameWidth / 2f;
        _paddle.Y = PaddleY + PaddleHeight / 2f;
        _paddle.SetWidthImmediate(DefaultPaddleWidth);
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
        _ball.Rigidbody.SetVelocity(
            (float)(BallSpeed * Math.Cos(angle)),
            -(float)(BallSpeed * Math.Sin(angle)));
    }

    // ── Input ─────────────────────────────────────────────────────────────

    public override void OnPointerMove(float x, float y)
    {
        float halfW = _paddle.Width / 2f;
        _paddle.X = Math.Clamp(x, halfW, GameWidth - halfW);
    }

    // ── Update ────────────────────────────────────────────────────────────

    public override void Update(float deltaTime)
    {
        _paddle.Update(deltaTime);

        foreach (var brick in _bricks)
            brick.Sprite.Update(deltaTime);

        // Power-up timers
        _strongBallTimer.Tick(deltaTime);

        if (_bigPaddleTimer.Tick(deltaTime))
        {
            _paddle.AnimateWidth(DefaultPaddleWidth, 0.4f, Easing.EaseIn);
            float halfW = DefaultPaddleWidth / 2f;
            _paddle.X = Math.Clamp(_paddle.X, halfW, GameWidth - halfW);
        }

        // Physics step
        _ball.Rigidbody.Step(_ball, deltaTime);

        // Boundary wall collisions (left, top, right bounce; bottom = life lost)
        foreach (var wall in (ReadOnlySpan<Wall>)[_leftWall, _topWall, _rightWall])
        {
            if (CollisionResolver.TryGetHit(_ball, _ball.Collider, wall, wall.Collider, out var wallHit))
                _ball.Rigidbody.Bounce(wallHit);
        }

        // Ball hit the bottom wall — lose a life
        if (CollisionResolver.Overlaps(_ball, _ball.Collider, _bottomWall, _bottomWall.Collider))
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
        ResolveBrickCollisions();

        // Falling power-ups
        UpdateFallingPowerUps(deltaTime);

        if (!_bricks.Any(b => b.Active))
            coordinator.PushOverlay<VictoryScreen>();
    }

    private void ResolvePaddleCollision()
    {
        if (_ball.Rigidbody.VelocityY > 0f &&
            CollisionResolver.TryGetHit(_ball, _ball.Collider, _paddle, _paddle.Collider, out _))
        {
            float hitPos = (_ball.X - _paddle.X) / (_paddle.Width / 2f);
            float angle = hitPos * (65f * MathF.PI / 180f);
            _ball.Rigidbody.SetVelocity(
                BallSpeed * MathF.Sin(angle),
                -BallSpeed * MathF.Cos(angle));
        }
    }

    private void ResolveBrickCollisions()
    {
        bool piercing = _strongBallTimer.Active;

        foreach (var brick in _bricks)
        {
            if (!brick.Active ||
                !CollisionResolver.TryGetHit(_ball, _ball.Collider, brick, brick.Collider, out var hit))
                continue;

            brick.Active = false;
            state.Score += 10 * (BrickRows - brick.Row);
            TrySpawnPowerUp(brick.X, brick.Y);

            if (!piercing)
            {
                _ball.Rigidbody.Bounce(hit);
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
        float halfPaddleW = _paddle.Width / 2f;

        for (int i = _powerUps.Count - 1; i >= 0; i--)
        {
            var pu = _powerUps[i];

            pu.Rigidbody.Step(pu, deltaTime);

            // Paddle collision: power-up centre Y within paddle vertical range, X within paddle width
            if (pu.Y + PowerUpH / 2f >= PaddleY &&
                pu.Y - PowerUpH / 2f <= PaddleY + PaddleHeight &&
                pu.X >= _paddle.X - halfPaddleW && pu.X <= _paddle.X + halfPaddleW)
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
                _strongBallTimer.Set(StrongBallDuration);
                break;
            case PowerUpType.BigPaddle:
                _bigPaddleTimer.Set(BigPaddleDuration);
                _paddle.AnimateWidth(DefaultPaddleWidth * BigPaddleMultiplier, 0.3f, Easing.BackOut);
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
            _powerUpLabel.Text = pu.Type == PowerUpType.StrongBall ? "S" : "B";
            _powerUpLabel.Draw(canvas, pu.X, pu.Y + 4f);
        }

        // Paddle
        _paddle.Sprite.Color = _bigPaddleTimer.Active || _paddle.IsWidthAnimating
            ? BigPaddleColor : PaddleColor;
        _paddle.Sprite.Draw(canvas, _paddle.X, _paddle.Y);

        // Ball
        _ball.Sprite.Color = _strongBallTimer.Active ? StrongBallColor : SKColors.White;
        _ball.Sprite.GlowColor = _strongBallTimer.Active ? StrongBallColor : SKColors.White;
        _ball.Sprite.Draw(canvas, _ball.X, _ball.Y);

        // HUD
        _scoreText.Text = $"Score: {state.Score}";
        _scoreText.Draw(canvas, 20f, 30f);

        _livesText.Text = $"Lives: {state.Lives}";
        _livesText.Draw(canvas, GameWidth - 20f, 30f);

        float hudY = 52f;
        if (_strongBallTimer.Active)
        {
            _strongBallText.Text = $"STRONG BALL {_strongBallTimer.Remaining:F1}s";
            _strongBallText.Color = StrongBallColor;
            _strongBallText.Draw(canvas, GameWidth / 2f, hudY);
            hudY += 18f;
        }
        if (_bigPaddleTimer.Active)
        {
            _bigPaddleText.Text = $"BIG PADDLE {_bigPaddleTimer.Remaining:F1}s";
            _bigPaddleText.Color = BigPaddleColor;
            _bigPaddleText.Draw(canvas, GameWidth / 2f, hudY);
        }
    }
}
