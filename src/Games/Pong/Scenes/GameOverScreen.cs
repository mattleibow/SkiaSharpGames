using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class GameOverScreen : Scene
{
    private readonly PongGameState state;
    private readonly IDirector director;

    private static readonly SKPaint _overlayPaint = new() { IsAntialias = true };

    private readonly HudLabel _winnerText = new()
    {
        FontSize = 58f,
        Color = AccentColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 260f,
    };
    private readonly HudLabel _scoreText = new()
    {
        FontSize = 42f,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 325f,
    };
    private readonly HudLabel _restartText = new()
    {
        Text = "Click, tap, or press Space to play again",
        FontSize = 24f,
        Color = DimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 395f,
    };

    public GameOverScreen(PongGameState state, IDirector director)
    {
        this.state = state;
        this.director = director;

        Children.Add(_winnerText);
        Children.Add(_scoreText);
        Children.Add(_restartText);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        _overlayPaint.Color = SKColors.Black.WithAlpha((byte)(255 * 0.76f));
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);

        _winnerText.Text = state.WinnerText;
        _scoreText.Text = $"{state.LeftScore} : {state.RightScore}";
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
