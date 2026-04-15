using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;
using TR = SkiaSharpGames.SinkSub.TextRenderer;

namespace SkiaSharpGames.SinkSub;

internal sealed class StartScreen(IScreenCoordinator coordinator) : GameScreen
{
    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(SkyColor);
        TR.FillRect(canvas, 0f, WaterlineY, GameWidth, GameHeight - WaterlineY, WaterColor);
        TR.FillRect(canvas, 0f, WaterlineY + 180f, GameWidth, GameHeight - WaterlineY - 180f, DeepWaterColor, 0.6f);

        TR.DrawCenteredText(canvas, "SINK SUB", 72f, SKColors.White, GameWidth / 2f, 215f);
        TR.DrawCenteredText(canvas, "Hunt submarines and avoid rising mines", 26f, AccentColor, GameWidth / 2f, 270f);
        TR.DrawCenteredText(canvas, "Move with mouse, touch, or arrow keys", 20f, DimColor, GameWidth / 2f, 345f);
        TR.DrawCenteredText(canvas, "Press Z or X to drop depth charges from the ship sides", 20f, DimColor, GameWidth / 2f, 375f);
        TR.DrawCenteredText(canvas, "Only four depth charges may be active at once", 20f, DimColor, GameWidth / 2f, 405f);
        TR.DrawCenteredText(canvas, "Click, tap, or press Space to begin", 24f, SKColors.White, GameWidth / 2f, 465f);
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<PlayScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveTransition());
    }
}
