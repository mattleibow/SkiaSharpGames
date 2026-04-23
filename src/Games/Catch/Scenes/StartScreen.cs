using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class StartScreen : Scene
{
    private readonly IDirector director;

    private readonly HudLabel _title = new() { Text = "CATCH", FontSize = 78f, Align = TextAlign.Center, X = GameWidth / 2f, Y = 225f };
    private readonly HudLabel _subtitle = new() { Text = "Catch the falling circles", FontSize = 30f, Color = AccentColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 285f };
    private readonly HudLabel _instruction1 = new() { Text = "Move the bar left and right", FontSize = 22f, Color = DimColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 350f };
    private readonly HudLabel _instruction2 = new() { Text = "Each catch speeds up the next drop", FontSize = 22f, Color = DimColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 382f };
    private readonly HudLabel _instruction3 = new() { Text = "You have 3 lives", FontSize = 22f, Color = DimColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 414f };
    private readonly HudLabel _startPrompt = new() { Text = "Click, tap, or press Space to start", FontSize = 24f, Align = TextAlign.Center, X = GameWidth / 2f, Y = 474f };

    public StartScreen(IDirector director)
    {
        this.director = director;

        Children.Add(_title);
        Children.Add(_subtitle);
        Children.Add(_instruction1);
        Children.Add(_instruction2);
        Children.Add(_instruction3);
        Children.Add(_startPrompt);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
