using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class StartScreen(IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _fillPaint = new() { IsAntialias = true };

    private readonly UiLabel _title = new() { Text = "SINK SUB", FontSize = 72f, Align = TextAlign.Center };
    private readonly UiLabel _subtitle = new() { Text = "Hunt submarines and avoid rising mines", FontSize = 26f, Color = AccentColor, Align = TextAlign.Center };
    private readonly UiLabel _instruction1 = new() { Text = "Move with mouse, touch, or arrow keys", FontSize = 20f, Color = DimColor, Align = TextAlign.Center };
    private readonly UiLabel _instruction2 = new() { Text = "Press Z or X to drop depth charges from the ship sides", FontSize = 20f, Color = DimColor, Align = TextAlign.Center };
    private readonly UiLabel _instruction3 = new() { Text = "Only four depth charges may be active at once", FontSize = 20f, Color = DimColor, Align = TextAlign.Center };
    private readonly UiLabel _startPrompt = new() { Text = "Click, tap, or press Space to begin", FontSize = 24f, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(SkyColor);

        _fillPaint.Color = WaterColor;
        canvas.DrawRect(SKRect.Create(0f, WaterlineY, GameWidth, GameHeight - WaterlineY), _fillPaint);
        _fillPaint.Color = DeepWaterColor.WithAlpha((byte)(255 * 0.6f));
        canvas.DrawRect(SKRect.Create(0f, WaterlineY + 180f, GameWidth, GameHeight - WaterlineY - 180f), _fillPaint);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 215f); _title.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 270f); _subtitle.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 345f); _instruction1.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 375f); _instruction2.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 405f); _instruction3.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 465f); _startPrompt.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<PlayScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveTransition());
    }
}
