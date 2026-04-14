using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

/// <summary>
/// Defeat overlay drawn on top of the frozen play screen.
/// Does not clear the canvas — relies on the base play screen being drawn first.
/// </summary>
internal sealed class CastleAttackGameOverScreen(CastleAttackGameState state, IScreenCoordinator coordinator) : GameScreen
{
    public override void Draw(SKCanvas canvas, int width, int height)
    {
        DrawHelper.DrawOverlay(canvas, GameWidth, GameHeight, 0.75f);
        DrawHelper.DrawCenteredText(canvas, "DEFEAT", 72f, ColRed, GameWidth / 2f, 250f);
        DrawHelper.DrawCenteredText(canvas, $"Score: {state.Score}", 32f, ColHud, GameWidth / 2f, 315f);
        DrawHelper.DrawCenteredText(canvas, "Click or Tap to Try Again", 22f, ColAccent, GameWidth / 2f, 370f);
    }

    public override void OnPointerDown(float x, float y)
        => coordinator.TransitionTo<CastleAttackStartScreen>(new DissolveTransition());
}
