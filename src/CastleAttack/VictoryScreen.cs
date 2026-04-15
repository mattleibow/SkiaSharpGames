using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>
/// Victory overlay drawn on top of the frozen play screen.
/// Does not clear the canvas — relies on the base play screen being drawn first.
/// </summary>
internal sealed class VictoryScreen(CastleAttackGameState state, IScreenCoordinator coordinator) : GameScreen
{
    public override void Draw(SKCanvas canvas, int width, int height)
    {
        TextRenderer.DrawOverlay(canvas, GameWidth, GameHeight, 0.75f);
        TextRenderer.DrawCenteredText(canvas, "VICTORY!", 72f, ColGold, GameWidth / 2f, 250f);
        TextRenderer.DrawCenteredText(canvas, $"Score: {state.Score}", 32f, ColHud, GameWidth / 2f, 315f);
        TextRenderer.DrawCenteredText(canvas, "The keep is complete!", 22f, ColAccent, GameWidth / 2f, 360f);
        TextRenderer.DrawCenteredText(canvas, "Click or Tap to Play Again", 22f, ColDim, GameWidth / 2f, 395f);
    }

    public override void OnPointerDown(float x, float y)
        => coordinator.TransitionTo<StartScreen>(new DissolveTransition());
}
