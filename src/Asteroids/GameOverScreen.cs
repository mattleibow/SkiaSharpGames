using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class GameOverScreen(AsteroidsGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha((byte)(255 * 0.8f)), IsAntialias = true };

    private readonly UiLabel _titleText = new() { Text = "GAME OVER", FontSize = 76f, Color = new SKColor(0xFF, 0x6B, 0x6B), Align = TextAlign.Center };
    private readonly UiLabel _scoreText = new() { FontSize = 32f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly UiLabel _levelText = new() { FontSize = 24f, Color = HudDimColor, Align = TextAlign.Center };
    private readonly UiLabel _restartText = new() { Text = "Click, tap, or press Space to play again", FontSize = 24f, Color = HudDimColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 235f); _titleText.Draw(canvas); canvas.Restore();

        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 310f); _scoreText.Draw(canvas); canvas.Restore();

        _levelText.Text = $"Reached Level {state.Level}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 355f); _levelText.Draw(canvas); canvas.Restore();

        canvas.Save(); canvas.Translate(GameWidth / 2f, 430f); _restartText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<StartScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveTransition());
    }
}
