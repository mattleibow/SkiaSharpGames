using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class VictoryScreen(SpaceInvadersGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _overlayPaint = new() { Color = new SKColor(0x00, 0x1A, 0x10).WithAlpha((byte)(255 * 0.8f)), IsAntialias = true };

    private readonly UiLabel _titleText = new() { Text = "WAVE CLEARED", FontSize = 72f, Color = AccentColor, Align = TextAlign.Center };
    private readonly UiLabel _scoreText = new() { FontSize = 32f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly UiLabel _restartText = new() { Text = "Click, tap, or press Space to defend again", FontSize = 24f, Color = HudDimColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 250f); _titleText.Draw(canvas); canvas.Restore();

        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 320f); _scoreText.Draw(canvas); canvas.Restore();

        canvas.Save(); canvas.Translate(GameWidth / 2f, 410f); _restartText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<StartScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveTransition());
    }
}
