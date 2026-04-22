using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class PlayScreen(PongGameState state, IDirector coordinator) : Scene
{
    private static readonly SKPaint _fillPaint = new() { IsAntialias = true };

    private readonly PongPaddle _leftPaddle = new(LeftPaddleColor);
    private readonly PongPaddle _rightPaddle = new(RightPaddleColor);
    private readonly PongBall _ball = new();
    private readonly PongEdge _topEdge = new(GameWidth * 0.5f, -EdgeColliderThickness * 0.5f, GameWidth, EdgeColliderThickness);
    private readonly PongEdge _bottomEdge = new(GameWidth * 0.5f, GameHeight + EdgeColliderThickness * 0.5f, GameWidth, EdgeColliderThickness);
    private readonly PongEdge _leftGoalEdge = new(-EdgeColliderThickness * 0.5f, GameHeight * 0.5f, EdgeColliderThickness, GameHeight);
    private readonly PongEdge _rightGoalEdge = new(GameWidth + EdgeColliderThickness * 0.5f, GameHeight * 0.5f, EdgeColliderThickness, GameHeight);
    private int _serveDirection = 1;

    private CountdownTimer _serveTimer;

    // HUD text labels
    private readonly UiLabel _leftScoreText = new() { FontSize = 64f, Align = TextAlign.Center };
    private readonly UiLabel _rightScoreText = new() { FontSize = 64f, Align = TextAlign.Center };
    private readonly UiLabel _leftControlsText = new() { Text = "W / S", FontSize = 18f, Color = LeftPaddleColor };
    private readonly UiLabel _rightControlsText = new() { Text = "Up / Dn", FontSize = 18f, Color = RightPaddleColor, Align = TextAlign.Right };
    private readonly UiLabel _serveText = new() { Text = "Serve!", FontSize = 26f, Color = AccentColor, Align = TextAlign.Center };

    public override void OnActivated()
    {
        state.LeftScore = 0;
        state.RightScore = 0;
        state.WinnerText = string.Empty;

        _leftPaddle.X = PaddleMargin + PaddleWidth * 0.5f;
        _leftPaddle.Y = GameHeight * 0.5f;
        _rightPaddle.X = GameWidth - PaddleMargin - PaddleWidth * 0.5f;
        _rightPaddle.Y = GameHeight * 0.5f;

        _leftPaddle.UpHeld = false;
        _leftPaddle.DownHeld = false;
        _rightPaddle.UpHeld = false;
        _rightPaddle.DownHeld = false;

        BeginServe(Random.Shared.Next(2) == 0 ? -1 : 1);
    }

    public override void OnPointerMove(float x, float y)
    {
        float clamped = Math.Clamp(y, PaddleHeight * 0.5f, GameHeight - PaddleHeight * 0.5f);
        if (x < GameWidth * 0.5f)
            _leftPaddle.Y = clamped;
        else
            _rightPaddle.Y = clamped;
    }

    public override void OnKeyDown(string key)
    {
        switch (key)
        {
            case "w":
            case "W":
                _leftPaddle.UpHeld = true;
                break;
            case "s":
            case "S":
                _leftPaddle.DownHeld = true;
                break;
            case "ArrowUp":
                _rightPaddle.UpHeld = true;
                break;
            case "ArrowDown":
                _rightPaddle.DownHeld = true;
                break;
        }
    }

    public override void OnKeyUp(string key)
    {
        switch (key)
        {
            case "w":
            case "W":
                _leftPaddle.UpHeld = false;
                break;
            case "s":
            case "S":
                _leftPaddle.DownHeld = false;
                break;
            case "ArrowUp":
                _rightPaddle.UpHeld = false;
                break;
            case "ArrowDown":
                _rightPaddle.DownHeld = false;
                break;
        }
    }

    public override void Update(float deltaTime)
    {
        _leftPaddle.Update(deltaTime);
        _rightPaddle.Update(deltaTime);

        if (_serveTimer.Tick(deltaTime))
        {
            float angle = (float)((Random.Shared.NextDouble() * 0.8 - 0.4) * Math.PI);
            _ball.Rigidbody.SetVelocity(
                _serveDirection * BallSpeed * MathF.Cos(angle),
                BallSpeed * MathF.Sin(angle));
        }

        if (_serveTimer.Active)
            return;

        _ball.Update(deltaTime);

        ResolveEdgeCollision(_topEdge, _ball.Rigidbody.VelocityY < 0f);
        ResolveEdgeCollision(_bottomEdge, _ball.Rigidbody.VelocityY > 0f);

        ResolvePaddleCollision(isLeft: true);
        ResolvePaddleCollision(isLeft: false);

        if (_ball.Rigidbody.VelocityX < 0f && _ball.TryGetHit(_leftGoalEdge, out _))
        {
            state.RightScore++;
            HandleScore(leftScored: false);
        }
        else if (_ball.Rigidbody.VelocityX > 0f && _ball.TryGetHit(_rightGoalEdge, out _))
        {
            state.LeftScore++;
            HandleScore(leftScored: true);
        }
    }

    private void ResolveEdgeCollision(PongEdge edge, bool movingTowardEdge)
    {
        if (!movingTowardEdge || !_ball.TryGetHit(edge, out var hit))
            return;

        _ball.X += hit.NormalX * hit.Penetration;
        _ball.Y += hit.NormalY * hit.Penetration;
        _ball.Rigidbody.Bounce(hit);
    }

    private void ResolvePaddleCollision(bool isLeft)
    {
        var paddle = isLeft ? _leftPaddle : _rightPaddle;
        bool movingTowardPaddle = isLeft ? _ball.Rigidbody.VelocityX < 0f : _ball.Rigidbody.VelocityX > 0f;
        if (!movingTowardPaddle || !_ball.TryGetHit(paddle, out var hit))
            return;

        _ball.X += hit.NormalX * hit.Penetration;
        _ball.Y += hit.NormalY * hit.Penetration;

        float hitOffset = Math.Clamp((_ball.Y - paddle.Y) / (PaddleHeight * 0.5f), -1f, 1f);
        _ball.Rigidbody.AddVelocity(0f, hitOffset * BallVerticalSpeedFromHit);

        float vx = _ball.Rigidbody.VelocityX;
        float vy = _ball.Rigidbody.VelocityY;
        vx = isLeft ? MathF.Abs(vx) : -MathF.Abs(vx);
        float speed = MathF.Sqrt(vx * vx + vy * vy);
        speed = Math.Clamp(speed + BallSpeedGain, BallSpeed, BallMaxSpeed);
        float inv = 1f / MathF.Max(speed, 0.001f);
        float nx = vx * inv;
        float ny = vy * inv;
        _ball.Rigidbody.SetVelocity(nx * speed, ny * speed);
    }

    private void HandleScore(bool leftScored)
    {
        if (state.LeftScore >= WinningScore || state.RightScore >= WinningScore)
        {
            state.WinnerText = state.LeftScore > state.RightScore
                ? "Left Player Wins!"
                : "Right Player Wins!";
            coordinator.PushOverlay<GameOverScreen>();
            return;
        }

        BeginServe(leftScored ? -1 : 1);
    }

    private void BeginServe(int direction)
    {
        _serveDirection = direction;
        _ball.X = GameWidth * 0.5f;
        _ball.Y = GameHeight * 0.5f;
        _ball.Rigidbody.Stop();
        _serveTimer.Set(ServeDelay);
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        _leftPaddle.Draw(canvas);
        _rightPaddle.Draw(canvas);

        // Center-court dashed line
        _fillPaint.Color = SKColors.White.WithAlpha((byte)(255 * 0.5f));
        for (float y = 18f; y < GameHeight; y += 30f)
            canvas.DrawRect(SKRect.Create(GameWidth / 2f - 3f, y, 6f, 16f), _fillPaint);

        _ball.Draw(canvas);

        // Scores
        _leftScoreText.Text = state.LeftScore.ToString();
        canvas.Save(); canvas.Translate(GameWidth * 0.34f, 82f); _leftScoreText.Draw(canvas); canvas.Restore();
        _rightScoreText.Text = state.RightScore.ToString();
        canvas.Save(); canvas.Translate(GameWidth * 0.66f, 82f); _rightScoreText.Draw(canvas); canvas.Restore();

        // Control hints
        canvas.Save(); canvas.Translate(18f, 28f); _leftControlsText.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth - 18f, 28f); _rightControlsText.Draw(canvas); canvas.Restore();

        if (_serveTimer.Active)
        {
            canvas.Save(); canvas.Translate(GameWidth / 2f, 320f); _serveText.Draw(canvas); canvas.Restore();
        }
    }
}
