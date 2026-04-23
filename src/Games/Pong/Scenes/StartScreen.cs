using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class StartScreen : Scene
{
    private readonly IDirector director;

    private readonly HudLabel _title = new() { Text = "2 PLAYER PONG", FontSize = 70f, Align = TextAlign.Center, X = GameWidth / 2f, Y = 235f };
    private readonly HudLabel _leftControls = new() { Text = "Left: W / S", FontSize = 26f, Color = LeftPaddleColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 305f };
    private readonly HudLabel _rightControls = new() { Text = "Right: Up / Dn", FontSize = 26f, Color = RightPaddleColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 340f };
    private readonly HudLabel _touchHint = new() { Text = "Touch or mouse: move the paddle on that side", FontSize = 20f, Color = DimColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 392f };
    private readonly HudLabel _scoreHint = new() { Text = "First to 7 points wins", FontSize = 22f, Color = AccentColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 430f };
    private readonly HudLabel _startPrompt = new() { Text = "Click, tap, or press Space to start", FontSize = 24f, Align = TextAlign.Center, X = GameWidth / 2f, Y = 480f };

    public StartScreen(IDirector director)
    {
        this.director = director;

        Children.Add(_title);
        Children.Add(_leftControls);
        Children.Add(_rightControls);
        Children.Add(_touchHint);
        Children.Add(_scoreHint);
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
