using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>Active gameplay screen: ball, paddle, bricks, power-ups, HUD.</summary>
internal sealed class PlayScreen(BreakoutGameState state, IDirector coordinator) : Scene
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

    // ── Keyboard tracking ──────────────────────────────────────────────────
    private bool _leftHeld, _rightHeld;

    // ── Actor groups (parenting) ─────────────────────────────────────────
    private readonly Actor _bricks = new();
    private readonly Actor _powerUps = new();

    // ── Text sprites ──────────────────────────────────────────────────────
    private readonly UiLabel _scoreText = new() { FontSize = 20f, Color = SKColors.White };
    private readonly UiLabel _livesText = new() { FontSize = 20f, Color = SKColors.White, Align = TextAlign.Right };
    private readonly UiLabel _powerUpLabel = new() { FontSize = 11f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly UiLabel _strongBallText = new() { FontSize = 14f, Align = TextAlign.Center };
    private readonly UiLabel _bigPaddleText = new() { FontSize = 14f, Align = TextAlign.Center };

    public override void OnActivating()
    {
        state.Score = 0;
        state.Lives = 3;
        _strongBallTimer = default;
        _bigPaddleTimer = default;

        // Clear and rebuild power-ups
        foreach (var child in _powerUps.Children.ToArray())
            _powerUps.RemoveChild(child);

        InitBricks();
        _paddle.X = GameWidth / 2f;
        _paddle.Y = PaddleY + PaddleHeight / 2f;
        _paddle.SetWidthImmediate(DefaultPaddleWidth);
        ResetBall();
    }

    public override void OnActivated()
    {
        ResetBall();
    }

    // ── Initialisation ────────────────────────────────────────────────────

    private void InitBricks()
    {
        // Remove all existing brick children
        foreach (var child in _bricks.Children.ToArray())
            _bricks.RemoveChild(child);

        for (int r = 0; r < BrickRows; r++)
        {
            for (int c = 0; c < BrickCols; c++)
            {
                float cx = BricksStartX + c * (BrickWidth + BrickGap) + BrickWidth / 2f;
                float cy = BricksStartY + r * (BrickHeight + BrickGap) + BrickHeight / 2f;
                var brick = new Brick(r, c, cx, cy);
                brick.Color = BrickColors[r];
                brick.Shimmer.Start(Random.Shared.NextSingle() * brick.Shimmer.Period);
                _bricks.AddChild(brick);
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

    public override void OnKeyDown(string key)
    {
        switch (key)
        {
            case "ArrowLeft": _leftHeld = true; break;
            case "ArrowRight": _rightHeld = true; break;
        }
    }

    public override void OnKeyUp(string key)
    {
        switch (key)
        {
            case "ArrowLeft": _leftHeld = false; break;
            case "ArrowRight": _rightHeld = false; break;
        }
    }

    // ── Update ────────────────────────────────────────────────────────────

    public override void Update(float deltaTime)
    {
        // Keyboard-driven paddle movement
        if (_leftHeld ^ _rightHeld)
        {
            float direction = _leftHeld ? -1f : 1f;
            float halfW = _paddle.Width / 2f;
            _paddle.X = Math.Clamp(_paddle.X + direction * PaddleSpeed * deltaTime, halfW, GameWidth - halfW);
        }

        // Actor.Update handles rigidbody step + sprite update via OnUpdate
        _paddle.Update(deltaTime);
        _ball.Update(deltaTime);
        _bricks.Update(deltaTime);

        // Power-up timers
        _strongBallTimer.Tick(deltaTime);

        if (_bigPaddleTimer.Tick(deltaTime))
        {
            _paddle.AnimateWidth(DefaultPaddleWidth, 0.4f, Easing.EaseIn);
            float halfW = DefaultPaddleWidth / 2f;
            _paddle.X = Math.Clamp(_paddle.X, halfW, GameWidth - halfW);
        }

        // Boundary wall collisions
        _ball.BounceOff(_leftWall);
        _ball.BounceOff(_topWall);
        _ball.BounceOff(_rightWall);

        // Ball hit the bottom wall — lose a life
        if (_ball.Overlaps(_bottomWall))
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

        if (!_bricks.Children.Any(b => b.Active))
            coordinator.PushOverlay<VictoryScreen>();
    }

    private void ResolvePaddleCollision()
    {
        if (_ball.Rigidbody.VelocityY > 0f && _ball.TryGetHit(_paddle, out var hit))
        {
            _ball.X += hit.NormalX * hit.Penetration;
            _ball.Y += hit.NormalY * hit.Penetration;

            float hitPos = Math.Clamp((_ball.WorldX - _paddle.WorldX) / (_paddle.Width / 2f), -1f, 1f);
            float angle = hitPos * (65f * MathF.PI / 180f);
            _ball.Rigidbody.SetVelocity(
                BallSpeed * MathF.Sin(angle),
                -BallSpeed * MathF.Cos(angle));
        }
    }

    private void ResolveBrickCollisions()
    {
        bool piercing = _strongBallTimer.Active;

        // Use FindChildCollision for broad-phase + narrow-phase
        while (true)
        {
            if (_bricks.FindChildCollision(_ball, out var hit) is not Brick brick)
                break;

            brick.Active = false;
            state.Score += 10 * (BrickRows - brick.Row);
            TrySpawnPowerUp(brick.X, brick.Y);

            if (!piercing)
            {
                _ball.Rigidbody.Bounce(hit);
                break;
            }
        }
    }

    private void TrySpawnPowerUp(float cx, float cy)
    {
        if (Random.Shared.NextDouble() >= PowerUpChance) return;
        var type = Random.Shared.NextDouble() < 0.5 ? PowerUpType.StrongBall : PowerUpType.BigPaddle;
        var pu = new FallingPowerUp { X = cx, Y = cy, Type = type };
        pu.Color = type == PowerUpType.StrongBall ? StrongBallColor : BigPaddleColor;
        _powerUps.AddChild(pu);
    }

    private void UpdateFallingPowerUps(float deltaTime)
    {
        _powerUps.Update(deltaTime);
        float halfPaddleW = _paddle.Width / 2f;

        foreach (var child in _powerUps.Children.ToArray())
        {
            if (child is not FallingPowerUp pu || !pu.Active) continue;

            if (pu.Y + PowerUpH / 2f >= PaddleY &&
                pu.Y - PowerUpH / 2f <= PaddleY + PaddleHeight &&
                pu.X >= _paddle.X - halfPaddleW && pu.X <= _paddle.X + halfPaddleW)
            {
                ApplyPowerUp(pu.Type);
                _powerUps.RemoveChild(pu);
            }
            else if (pu.Y > GameHeight + 30f)
            {
                _powerUps.RemoveChild(pu);
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
        // Actor groups draw all children recursively
        _bricks.Draw(canvas);
        _powerUps.Draw(canvas);

        // Draw power-up labels on top of power-up entities
        foreach (var child in _powerUps.Children)
        {
            if (child is not FallingPowerUp pu || !pu.Active) continue;
            _powerUpLabel.Text = pu.Type == PowerUpType.StrongBall ? "S" : "B";
            canvas.Save(); canvas.Translate(pu.X, pu.Y + 4f); _powerUpLabel.Draw(canvas); canvas.Restore();
        }

        // Paddle
        _paddle.Color = _bigPaddleTimer.Active || _paddle.IsWidthAnimating
            ? BigPaddleColor : PaddleColor;
        _paddle.Draw(canvas);

        // Ball
        _ball.Color = _strongBallTimer.Active ? StrongBallColor : SKColors.White;
        _ball.GlowColor = _strongBallTimer.Active ? StrongBallColor : SKColors.White;
        _ball.Draw(canvas);

        // HUD
        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(20f, 30f); _scoreText.Draw(canvas); canvas.Restore();

        _livesText.Text = $"Lives: {state.Lives}";
        canvas.Save(); canvas.Translate(GameWidth - 20f, 30f); _livesText.Draw(canvas); canvas.Restore();

        float hudY = 52f;
        if (_strongBallTimer.Active)
        {
            _strongBallText.Text = $"STRONG BALL {_strongBallTimer.Remaining:F1}s";
            _strongBallText.Color = StrongBallColor;
            canvas.Save(); canvas.Translate(GameWidth / 2f, hudY); _strongBallText.Draw(canvas); canvas.Restore();
            hudY += 18f;
        }
        if (_bigPaddleTimer.Active)
        {
            _bigPaddleText.Text = $"BIG PADDLE {_bigPaddleTimer.Remaining:F1}s";
            _bigPaddleText.Color = BigPaddleColor;
            canvas.Save(); canvas.Translate(GameWidth / 2f, hudY); _bigPaddleText.Draw(canvas); canvas.Restore();
        }
    }
}
