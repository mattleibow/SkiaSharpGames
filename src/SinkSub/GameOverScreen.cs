using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;
using TR = SkiaSharpGames.SinkSub.TextRenderer;

namespace SkiaSharpGames.SinkSub;

internal sealed class GameOverScreen(SinkSubGameState state, IScreenCoordinator coordinator) : GameScreen
{
    public override void Draw(SKCanvas canvas, int width, int height)
    {
        TR.DrawOverlay(canvas, GameWidth, GameHeight, 0.78f, SKColors.Black);
        TR.DrawCenteredText(canvas, "SHIP SUNK", 62f, new SKColor(0xFF, 0x73, 0x5A), GameWidth / 2f, 250f);
        TR.DrawCenteredText(canvas, $"Score: {state.Score}", 30f, SKColors.White, GameWidth / 2f, 320f);
        TR.DrawCenteredText(canvas, $"Wave reached: {state.Wave}", 24f, AccentColor, GameWidth / 2f, 360f);
        TR.DrawCenteredText(canvas, "Click, tap, or press Space to sail again", 22f, DimColor, GameWidth / 2f, 420f);
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<StartScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveTransition());
    }
}
