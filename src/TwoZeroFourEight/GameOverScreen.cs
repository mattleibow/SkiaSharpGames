using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.TwoZeroFourEight.TwoZeroFourEightConstants;

namespace SkiaSharpGames.TwoZeroFourEight;

internal sealed class GameOverScreen(TwoZeroFourEightGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha((byte)(255 * 0.65f)), IsAntialias = true };

    private readonly TextSprite _titleText = new() { Text = "GAME OVER", Size = 72f, Color = LightTextColor, Align = TextAlign.Center };
    private readonly TextSprite _scoreText = new() { Size = 30f, Color = LightTextColor, Align = TextAlign.Center };
    private readonly TextSprite _bestText = new() { Size = 28f, Color = LightTextColor, Align = TextAlign.Center };
    private readonly TextSprite _restartText = new() { Text = "Click, tap, Enter, or Space to restart", Size = 22f, Color = LightTextColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 250f); _titleText.Draw(canvas); canvas.Restore();

        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 320f); _scoreText.Draw(canvas); canvas.Restore();

        _bestText.Text = $"Best: {state.BestScore}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 360f); _bestText.Draw(canvas); canvas.Restore();

        canvas.Save(); canvas.Translate(GameWidth / 2f, 425f); _restartText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<PlayScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveTransition());
    }
}
