using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.TwoZeroFourEight.TwoZeroFourEightConstants;

namespace SkiaSharpGames.TwoZeroFourEight;

internal sealed class StartScreen(IDirector director) : Scene
{
    private readonly HudLabel _title = new() { Text = "2048", FontSize = 92f, Color = HeaderColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 180f };
    private readonly HudLabel _subtitle = new() { Text = "Slide tiles. Merge equal numbers. Reach 2048.", FontSize = 28f, Color = HeaderColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 250f };
    private readonly HudLabel _controls = new() { Text = "Arrow keys or swipe", FontSize = 24f, Color = HeaderColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 320f };
    private readonly HudLabel _startPrompt = new() { Text = "Click, tap, Enter, or Space to begin", FontSize = 24f, Color = HeaderColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 360f };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_title);
            Children.Add(_subtitle);
            Children.Add(_controls);
            Children.Add(_startPrompt);
        }
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
