using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class GameOverScreen(SinkSubGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _fillPaint = new() { IsAntialias = true };

    private readonly TextSprite _titleText = new() { Text = "SHIP SUNK", Size = 62f, Color = new SKColor(0xFF, 0x73, 0x5A), Align = TextAlign.Center };
    private readonly TextSprite _scoreText = new() { Size = 30f, Align = TextAlign.Center };
    private readonly TextSprite _waveText = new() { Size = 24f, Color = AccentColor, Align = TextAlign.Center };
    private readonly TextSprite _restartText = new() { Text = "Click, tap, or press Space to sail again", Size = 22f, Color = DimColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        _fillPaint.Color = SKColors.Black.WithAlpha((byte)(255 * 0.78f));
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _fillPaint);

        _titleText.Draw(canvas, GameWidth / 2f, 250f);

        _scoreText.Text = $"Score: {state.Score}";
        _scoreText.Draw(canvas, GameWidth / 2f, 320f);

        _waveText.Text = $"Wave reached: {state.Wave}";
        _waveText.Draw(canvas, GameWidth / 2f, 360f);

        _restartText.Draw(canvas, GameWidth / 2f, 420f);
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<StartScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveTransition());
    }
}
