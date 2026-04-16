using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class StartScreen(IScreenCoordinator coordinator) : GameScreen
{
    private readonly UiLabel _title = new() { Text = "CATCH", FontSize = 78f, Align = TextAlign.Center };
    private readonly UiLabel _subtitle = new() { Text = "Catch the falling circles", FontSize = 30f, Color = AccentColor, Align = TextAlign.Center };
    private readonly UiLabel _instruction1 = new() { Text = "Move the bar left and right", FontSize = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly UiLabel _instruction2 = new() { Text = "Each catch speeds up the next drop", FontSize = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly UiLabel _instruction3 = new() { Text = "You have 3 lives", FontSize = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly UiLabel _startPrompt = new() { Text = "Click, tap, or press Space to start", FontSize = 24f, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 225f); _title.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 285f); _subtitle.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 350f); _instruction1.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 382f); _instruction2.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 414f); _instruction3.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 474f); _startPrompt.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<PlayScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveTransition());
    }
}
