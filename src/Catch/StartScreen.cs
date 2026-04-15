using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class StartScreen(IScreenCoordinator coordinator) : GameScreen
{
    private readonly TextSprite _title = new() { Text = "CATCH", Size = 78f, Align = TextAlign.Center };
    private readonly TextSprite _subtitle = new() { Text = "Catch the falling circles", Size = 30f, Color = AccentColor, Align = TextAlign.Center };
    private readonly TextSprite _instruction1 = new() { Text = "Move the bar left and right", Size = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly TextSprite _instruction2 = new() { Text = "Each catch speeds up the next drop", Size = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly TextSprite _instruction3 = new() { Text = "You have 3 lives", Size = 22f, Color = DimColor, Align = TextAlign.Center };
    private readonly TextSprite _startPrompt = new() { Text = "Click, tap, or press Space to start", Size = 24f, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        _title.Draw(canvas, GameWidth / 2f, 225f);
        _subtitle.Draw(canvas, GameWidth / 2f, 285f);
        _instruction1.Draw(canvas, GameWidth / 2f, 350f);
        _instruction2.Draw(canvas, GameWidth / 2f, 382f);
        _instruction3.Draw(canvas, GameWidth / 2f, 414f);
        _startPrompt.Draw(canvas, GameWidth / 2f, 474f);
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<PlayScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveTransition());
    }
}
