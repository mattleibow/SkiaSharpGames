using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>
/// Defeat overlay drawn on top of the frozen play screen.
/// Does not clear the canvas — relies on the base play screen being drawn first.
/// </summary>
internal sealed class GameOverScreen(CastleAttackGameState state, IScreenCoordinator coordinator) : GameScreen
{
    public override void Draw(SKCanvas canvas, int width, int height)
    {
        TextRenderer.DrawOverlay(canvas, GameWidth, GameHeight, 0.75f);
        TextRenderer.DrawCenteredText(canvas, "DEFEAT", 72f, ColRed, GameWidth / 2f, 250f);
        TextRenderer.DrawCenteredText(canvas, $"Score: {state.Score}", 32f, ColHud, GameWidth / 2f, 315f);
        TextRenderer.DrawCenteredText(canvas, "Click or Tap to Try Again", 22f, ColAccent, GameWidth / 2f, 370f);
    }

    public override void OnPointerDown(float x, float y)
        => coordinator.TransitionTo<StartScreen>(new DissolveTransition());
}
