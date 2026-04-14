using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.BlazorApp.Games.SinkSub;

internal sealed class StartScreen(IScreenCoordinator coordinator) : GameScreen
{
    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(SkyColor);
        DrawHelper.FillRect(canvas, 0f, WaterlineY, GameWidth, GameHeight - WaterlineY, WaterColor);
        DrawHelper.FillRect(canvas, 0f, WaterlineY + 180f, GameWidth, GameHeight - WaterlineY - 180f, DeepWaterColor, 0.6f);

        DrawHelper.DrawCenteredText(canvas, "SINK SUB", 72f, SKColors.White, GameWidth / 2f, 215f);
        DrawHelper.DrawCenteredText(canvas, "Hunt submarines and avoid rising mines", 26f, AccentColor, GameWidth / 2f, 270f);
        DrawHelper.DrawCenteredText(canvas, "Move with mouse, touch, or arrow keys", 20f, DimColor, GameWidth / 2f, 345f);
        DrawHelper.DrawCenteredText(canvas, "Press Z or X to drop depth charges from the ship sides", 20f, DimColor, GameWidth / 2f, 375f);
        DrawHelper.DrawCenteredText(canvas, "Only four depth charges may be active at once", 20f, DimColor, GameWidth / 2f, 405f);
        DrawHelper.DrawCenteredText(canvas, "Click, tap, or press Space to begin", 24f, SKColors.White, GameWidth / 2f, 465f);
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<PlayScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveTransition());
    }
}
