using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class GameOverScreen(AsteroidsGameState state, IDirector director) : Scene
{
    private static readonly SKPaint _overlayPaint = new()
    {
        Color = SKColors.Black.WithAlpha((byte)(255 * 0.8f)),
        IsAntialias = true,
    };

    private readonly HudLabel _titleText = new()
    {
        Text = "GAME OVER",
        FontSize = 76f,
        Color = new SKColor(0xFF, 0x6B, 0x6B),
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 235f,
    };
    private readonly HudLabel _scoreText = new()
    {
        Name = "score",
        FontSize = 32f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 310f,
    };
    private readonly HudLabel _levelText = new()
    {
        Name = "level",
        FontSize = 24f,
        Color = HudDimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 355f,
    };
    private readonly HudLabel _restartText = new()
    {
        Text = "Click, tap, or press Space to play again",
        FontSize = 24f,
        Color = HudDimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 430f,
    };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_titleText);
            Children.Add(_scoreText);
            Children.Add(_levelText);
            Children.Add(_restartText);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);

        _scoreText.Text = $"Score: {state.Score}";
        _levelText.Text = $"Reached Level {state.Level}";
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
