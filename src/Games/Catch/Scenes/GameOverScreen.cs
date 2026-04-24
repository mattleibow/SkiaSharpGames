using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class GameOverScreen : Scene
{
    private readonly CatchGameState state;
    private readonly IDirector director;

    private static readonly SKPaint _overlayPaint = new()
    {
        Color = SKColors.Black.WithAlpha((byte)(255 * 0.8f)),
    };

    private readonly HudLabel _titleText = new()
    {
        Text = "GAME OVER",
        FontSize = 72f,
        Color = DangerColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 250f,
    };
    private readonly HudLabel _scoreText = new()
    {
        FontSize = 34f,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 325f,
    };
    private readonly HudLabel _promptText = new()
    {
        Text = "Click, tap, or press Space to try again",
        FontSize = 24f,
        Color = AccentColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 395f,
    };

    public GameOverScreen(CatchGameState state, IDirector director)
    {
        this.state = state;
        this.director = director;

        Children.Add(_titleText);
        Children.Add(_scoreText);
        Children.Add(_promptText);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _overlayPaint);

        _scoreText.Text = $"Score: {state.Score}";
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
