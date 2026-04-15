using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class PlayScreen(PongGameState state, IScreenCoordinator coordinator) : GameScreen
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

    private bool _leftUpHeld;
    private bool _leftDownHeld;
    private bool _rightUpHeld;
    private bool _rightDownHeld;

    private CountdownTimer _serveTimer;

    // HUD text sprites
    private readonly TextSprite _leftScoreText = new() { Size = 64f, Align = TextAlign.Center };
    private readonly TextSprite _rightScoreText = new() { Size = 64f, Align = TextAlign.Center };
    private readonly TextSprite _leftControlsText = new() { Text = "W / S", Size = 18f, Color = LeftPaddleColor };
    private readonly TextSprite _rightControlsText = new() { Text = "\u2191 / \u2193", Size = 18f, Color = RightPaddleColor, Align = TextAlign.Right };
    private readonly TextSprite _serveText = new() { Text = "Serve!", Size = 26f, Color = AccentColor, Align = TextAlign.Center };

    public override void OnActivated()
    {
        state.LeftScore = 0;
        state.RightScore = 0;
        state.WinnerText = string.Empty;

        _leftPaddle.X = PaddleMargin + PaddleWidth * 0.5f;
        _leftPaddle.Y = GameHeight * 0.5f;
        _rightPaddle.X = GameWidth - PaddleMargin - PaddleWidth * 0.5f;
        _rightPaddle.Y = GameHeight * 0.5f;

        _leftUpHeld = false;
        _leftDownHeld = false;
        _rightUpHeld = false;
        _rightDownHeld = false;

        BeginServe(Random.Shared.Next(2) == 0 ? -1 : 1);
    }

    public override void OnPointerMove(float x, float y)
    {
        if (x < GameWidth * 0.5f)
            _leftPaddle.Y = ClampPaddleY(y);
        else
            _rightPaddle.Y = ClampPaddleY(y);
    }

    public override void OnKeyDown(string key)
    {
        switch (key)
        {
            case "w":
            case "W":
                _leftUpHeld = true;
                break;
            case "s":
            case "S":
                _leftDownHeld = true;
                break;
            case "ArrowUp":
                _rightUpHeld = true;
                break;
            case "ArrowDown":
                _rightDownHeld = true;
                break;
        }
    }

    public override void OnKeyUp(string key)
    {
        switch (key)
        {
            case "w":
            case "W":
                _leftUpHeld = false;
                break;
            case "s":
            case "S":
                _leftDownHeld = false;
                break;
            case "ArrowUp":
                _rightUpHeld = false;
                break;
            case "ArrowDown":
                _rightDownHeld = false;
                break;
        }
    }

    public override void Update(float deltaTime)
    {
        UpdatePaddles(deltaTime);

        if (_serveTimer.Tick(deltaTime))
        {
            float angle = (float)((Random.Shared.NextDouble() * 0.8 - 0.4) * Math.PI);
            _ball.Rigidbody.SetVelocity(
                _serveDirection * BallSpeed * MathF.Cos(angle),
                BallSpeed * MathF.Sin(angle));
        }

        if (_serveTimer.Active)
            return;

        _ball.Rigidbody.Step(_ball, deltaTime);

        ResolveEdgeCollision(_topEdge, _ball.Rigidbody.VelocityY < 0f);
        ResolveEdgeCollision(_bottomEdge, _ball.Rigidbody.VelocityY > 0f);

        ResolvePaddleCollision(isLeft: true);
        ResolvePaddleCollision(isLeft: false);

        if (_ball.Rigidbody.VelocityX < 0f &&
            CollisionResolver.TryGetHit(_ball.X, _ball.Y, _ball.Collider, _leftGoalEdge.X, _leftGoalEdge.Y, _leftGoalEdge.Collider, out _))
        {
            state.RightScore++;
            HandleScore(leftScored: false);
        }
        else if (_ball.Rigidbody.VelocityX > 0f &&
            CollisionResolver.TryGetHit(_ball.X, _ball.Y, _ball.Collider, _rightGoalEdge.X, _rightGoalEdge.Y, _rightGoalEdge.Collider, out _))
        {
            state.LeftScore++;
            HandleScore(leftScored: true);
        }
    }

    private void UpdatePaddles(float deltaTime)
    {
        float leftMove = 0f;
        if (_leftUpHeld) leftMove -= PaddleSpeed * deltaTime;
        if (_leftDownHeld) leftMove += PaddleSpeed * deltaTime;
        _leftPaddle.Y = ClampPaddleY(_leftPaddle.Y + leftMove);

        float rightMove = 0f;
        if (_rightUpHeld) rightMove -= PaddleSpeed * deltaTime;
        if (_rightDownHeld) rightMove += PaddleSpeed * deltaTime;
        _rightPaddle.Y = ClampPaddleY(_rightPaddle.Y + rightMove);
    }

    private void ResolveEdgeCollision(PongEdge edge, bool movingTowardEdge)
    {
        if (!movingTowardEdge ||
            !CollisionResolver.TryGetHit(_ball.X, _ball.Y, _ball.Collider, edge.X, edge.Y, edge.Collider, out var hit))
            return;

        _ball.X += hit.NormalX * hit.Penetration;
        _ball.Y += hit.NormalY * hit.Penetration;
        _ball.Rigidbody.Bounce(hit);
    }

    private void ResolvePaddleCollision(bool isLeft)
    {
        var paddle = isLeft ? _leftPaddle : _rightPaddle;
        bool movingTowardPaddle = isLeft ? _ball.Rigidbody.VelocityX < 0f : _ball.Rigidbody.VelocityX > 0f;
        if (!movingTowardPaddle ||
            !CollisionResolver.TryGetHit(_ball.X, _ball.Y, _ball.Collider, paddle.X, paddle.Y, paddle.Collider, out var hit))
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

    private static float ClampPaddleY(float y) =>
        Math.Clamp(y, PaddleHeight * 0.5f, GameHeight - PaddleHeight * 0.5f);

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        canvas.Save(); canvas.Translate(_leftPaddle.X, _leftPaddle.Y); _leftPaddle.Sprite.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(_rightPaddle.X, _rightPaddle.Y); _rightPaddle.Sprite.Draw(canvas); canvas.Restore();

        // Center-court dashed line
        _fillPaint.Color = SKColors.White.WithAlpha((byte)(255 * 0.5f));
        for (float y = 18f; y < GameHeight; y += 30f)
            canvas.DrawRect(SKRect.Create(GameWidth / 2f - 3f, y, 6f, 16f), _fillPaint);

        canvas.Save(); canvas.Translate(_ball.X, _ball.Y); _ball.Sprite.Draw(canvas); canvas.Restore();

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
