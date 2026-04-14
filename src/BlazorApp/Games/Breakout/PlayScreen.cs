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

    // ── Power-up timers ───────────────────────────────────────────────────
    private CountdownTimer _strongBallTimer;
    private CountdownTimer _bigPaddleTimer;

    // True on the very first Update after activation so the ball can be re-synced
    // to wherever the paddle has moved during the dissolve-in transition.
    private bool _needsBallReset;

    private readonly List<Brick> _bricks = [];
    private readonly List<FallingPowerUp> _powerUps = [];

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
        // Flag so the first Update re-syncs the ball above wherever the paddle
        // actually ends up after the transition-in completes.
        _needsBallReset = true;
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

        // On the first Update after activation, re-sync the ball to the paddle's actual
        // position. The paddle may have been moved by pointer events that fired during the
        // dissolve-in transition (while Update was paused), which would otherwise leave the
        // ball stranded at the centre while the paddle sits at the cursor position.
        if (_needsBallReset)
        {
            _needsBallReset = false;
            ResetBall();
        }

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

        // Ball lost — check BEFORE wall resolution so the ball is not clamped at the bottom
        // boundary and the lost condition can be detected on the same frame.
        if (_ball.Y + _ball.Collider.Radius > GameHeight)
        {
            state.Lives--;
            if (state.Lives <= 0)
                coordinator.PushOverlay<GameOverScreen>();
            else
                ResetBall();
            return;
        }

        // Wall collisions (left / right / top only — bottom is an open boundary handled above)
        CollisionResolver.ResolveBounds(
            _ball,
            _ball.Collider,
            _ball.Rigidbody,
            GameBounds.FromSize(GameWidth, GameHeight),
            bounceBottom: false);

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
            CollisionResolver.TryGetHit(_ball, _ball.Collider, _paddle, _paddle.Collider, out var hit))
        {
            // Push the ball out of the paddle so it cannot oscillate inside it.
            _ball.X += hit.NormalX * hit.Penetration;
            _ball.Y += hit.NormalY * hit.Penetration;

            // Clamp hitPos to [-1, 1] so corner overlaps don't produce extreme angles.
            float hitPos = Math.Clamp((_ball.X - _paddle.X) / (_paddle.Width / 2f), -1f, 1f);
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
            string label = pu.Type == PowerUpType.StrongBall ? "S" : "B";
            DrawHelper.DrawCenteredText(canvas, label, 11f, SKColors.White, pu.X, pu.Y + 4f);
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
        DrawHelper.DrawText(canvas, $"Score: {state.Score}", 20f, SKColors.White, 20f, 30f);
        string livesText = $"Lives: {state.Lives}";
        float livesW = DrawHelper.MeasureText(livesText, 20f);
        DrawHelper.DrawText(canvas, livesText, 20f, SKColors.White, GameWidth - livesW - 20f, 30f);

        float hudY = 52f;
        if (_strongBallTimer.Active)
        {
            DrawHelper.DrawCenteredText(canvas, $"STRONG BALL {_strongBallTimer.Remaining:F1}s",
                14f, StrongBallColor, GameWidth / 2f, hudY);
            hudY += 18f;
        }
        if (_bigPaddleTimer.Active)
        {
            DrawHelper.DrawCenteredText(canvas, $"BIG PADDLE {_bigPaddleTimer.Remaining:F1}s",
                14f, BigPaddleColor, GameWidth / 2f, hudY);
        }
    }
}
