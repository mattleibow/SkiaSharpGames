using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class PlayScreen(CatchGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _fillPaint = new() { IsAntialias = true };

    private readonly PlayerBar _bar = new();
    private readonly FallingCircle _circle = new();

    private readonly TextSprite _scoreText = new() { Size = 24f };
    private readonly TextSprite _livesText = new() { Size = 24f, Align = TextAlign.Right };
    private readonly TextSprite _instructionsText = new() { Text = "Move with mouse, touch, or arrow keys", Size = 16f, Color = DimColor, Align = TextAlign.Center };

    private float _fallSpeed;
    private bool _leftHeld;
    private bool _rightHeld;
    private bool _gameOverShown;

    public override void OnActivated()
    {
        state.Score = 0;
        state.Lives = 3;

        _fallSpeed = InitialFallSpeed;
        _leftHeld = false;
        _rightHeld = false;
        _gameOverShown = false;

        _bar.X = GameWidth / 2f;
        _bar.Y = BarY;

        SpawnCircle();
    }

    public override void OnPointerMove(float x, float y) => MoveBarTo(x);

    public override void OnPointerDown(float x, float y) => MoveBarTo(x);

    public override void OnKeyDown(string key)
    {
        switch (key)
        {
            case "ArrowLeft":
                _leftHeld = true;
                break;
            case "ArrowRight":
                _rightHeld = true;
                break;
        }
    }

    public override void OnKeyUp(string key)
    {
        switch (key)
        {
            case "ArrowLeft":
                _leftHeld = false;
                break;
            case "ArrowRight":
                _rightHeld = false;
                break;
        }
    }

    public override void Update(float deltaTime)
    {
        if (_leftHeld)
            MoveBarTo(_bar.X - BarSpeed * deltaTime);
        if (_rightHeld)
            MoveBarTo(_bar.X + BarSpeed * deltaTime);

        _circle.Update(deltaTime);

        if (_circle.Rigidbody.VelocityY > 0f &&
            _circle.TryGetHit(_bar, out _))
        {
            state.Score++;
            _fallSpeed += FallSpeedIncrement;
            SpawnCircle();
            return;
        }

        if (_circle.Y - CircleRadius > GameHeight)
        {
            state.Lives--;

            if (state.Lives <= 0 && !_gameOverShown)
            {
                _gameOverShown = true;
                coordinator.PushOverlay<GameOverScreen>();
                return;
            }

            SpawnCircle();
        }
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        // Ground line
        _fillPaint.Color = SKColors.White.WithAlpha((byte)(255 * 0.09f));
        canvas.DrawRect(SKRect.Create(0f, BarY + BarHeight / 2f + 20f, GameWidth, 3f), _fillPaint);

        _bar.Draw(canvas);
        _circle.Draw(canvas);

        // HUD
        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(20f, 35f); _scoreText.Draw(canvas); canvas.Restore();

        _livesText.Text = $"Lives: {state.Lives}";
        _livesText.Color = state.Lives == 1 ? DangerColor : SKColors.White;
        canvas.Save(); canvas.Translate(GameWidth - 20f, 35f); _livesText.Draw(canvas); canvas.Restore();

        canvas.Save(); canvas.Translate(GameWidth / 2f, 34f); _instructionsText.Draw(canvas); canvas.Restore();
    }

    private void MoveBarTo(float x)
    {
        float half = BarWidth / 2f;
        _bar.X = Math.Clamp(x, half, GameWidth - half);
    }

    private void SpawnCircle()
    {
        _circle.X = CircleRadius + Random.Shared.NextSingle() * (GameWidth - CircleRadius * 2f);
        _circle.Y = -CircleRadius;
        _circle.Rigidbody.SetVelocity(0f, _fallSpeed);
    }
}
