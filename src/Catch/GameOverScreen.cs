using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class GameOverScreen(CatchGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha((byte)(255 * 0.8f)) };

    private readonly TextSprite _titleText = new() { Text = "GAME OVER", Size = 72f, Color = DangerColor, Align = TextAlign.Center };
    private readonly TextSprite _scoreText = new() { Size = 34f, Align = TextAlign.Center };
    private readonly TextSprite _promptText = new() { Text = "Click, tap, or press Space to try again", Size = 24f, Color = AccentColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _overlayPaint);

        _titleText.Draw(canvas, GameWidth / 2f, 250f);

        _scoreText.Text = $"Score: {state.Score}";
        _scoreText.Draw(canvas, GameWidth / 2f, 325f);

        _promptText.Draw(canvas, GameWidth / 2f, 395f);
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<StartScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveTransition());
    }
}
