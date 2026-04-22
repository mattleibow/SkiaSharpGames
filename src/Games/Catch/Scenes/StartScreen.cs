using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class StartScreen(IDirector director) : Scene
{
    private readonly HudLabel _title = new() { Text = "CATCH", FontSize = 78f, Align = TextAlign.Center, X = GameWidth / 2f, Y = 225f };
    private readonly HudLabel _subtitle = new() { Text = "Catch the falling circles", FontSize = 30f, Color = AccentColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 285f };
    private readonly HudLabel _instruction1 = new() { Text = "Move the bar left and right", FontSize = 22f, Color = DimColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 350f };
    private readonly HudLabel _instruction2 = new() { Text = "Each catch speeds up the next drop", FontSize = 22f, Color = DimColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 382f };
    private readonly HudLabel _instruction3 = new() { Text = "You have 3 lives", FontSize = 22f, Color = DimColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 414f };
    private readonly HudLabel _startPrompt = new() { Text = "Click, tap, or press Space to start", FontSize = 24f, Align = TextAlign.Center, X = GameWidth / 2f, Y = 474f };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        _title.Draw(canvas);
        _subtitle.Draw(canvas);
        _instruction1.Draw(canvas);
        _instruction2.Draw(canvas);
        _instruction3.Draw(canvas);
        _startPrompt.Draw(canvas);
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
