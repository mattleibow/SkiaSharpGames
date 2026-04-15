using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// Game-over overlay drawn on top of the frozen play screen.
/// Does not clear the canvas — relies on the base play screen being drawn first.
/// </summary>
internal sealed class GameOverScreen(BreakoutGameState state, IScreenCoordinator coordinator) : GameScreen
{
    public override void Draw(SKCanvas canvas, int width, int height)
    {
        TextRenderer.DrawOverlay(canvas, GameWidth, GameHeight);
        TextRenderer.DrawCenteredText(canvas, "GAME OVER", 64f, new SKColor(0xFF, 0x2D, 0x55), GameWidth / 2f, 270f);
        TextRenderer.DrawCenteredText(canvas, $"Score: {state.Score}", 32f, SKColors.White, GameWidth / 2f, 335f);
        TextRenderer.DrawCenteredText(canvas, "Click or Tap to Play Again", 24f, AccentColor, GameWidth / 2f, 395f);
    }

    public override void OnPointerDown(float x, float y)
        => coordinator.TransitionTo<StartScreen>(new DissolveTransition());
}
