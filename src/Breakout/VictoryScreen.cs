using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// Victory overlay drawn on top of the frozen play screen.
/// Does not clear the canvas — relies on the base play screen being drawn first.
/// </summary>
internal sealed class VictoryScreen(BreakoutGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha(186) };

    private readonly UiLabel _titleText = new() { Text = "YOU WIN!", FontSize = 64f, Color = new SKColor(0xFF, 0xD6, 0x0A), Align = TextAlign.Center };
    private readonly UiLabel _scoreText = new() { FontSize = 32f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly UiLabel _promptText = new() { Text = "Click or Tap to Play Again", FontSize = 24f, Color = AccentColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _overlayPaint);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 270f); _titleText.Draw(canvas); canvas.Restore();
        _scoreText.Text = $"Final Score: {state.Score}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 335f); _scoreText.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 395f); _promptText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y)
        => coordinator.TransitionTo<StartScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveTransition());
    }
}
