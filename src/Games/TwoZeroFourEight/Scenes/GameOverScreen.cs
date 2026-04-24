using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.TwoZeroFourEight.TwoZeroFourEightConstants;

namespace SkiaSharpGames.TwoZeroFourEight;

internal sealed class GameOverScreen(TwoZeroFourEightGameState state, IDirector director) : Scene
{
    private static readonly SKPaint _overlayPaint = new()
    {
        Color = SKColors.Black.WithAlpha((byte)(255 * 0.65f)),
        IsAntialias = true,
    };

    private readonly HudLabel _titleText = new()
    {
        Text = "GAME OVER",
        FontSize = 72f,
        Color = LightTextColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 250f,
    };
    private readonly HudLabel _scoreText = new()
    {
        FontSize = 30f,
        Color = LightTextColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 320f,
    };
    private readonly HudLabel _bestText = new()
    {
        FontSize = 28f,
        Color = LightTextColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 360f,
    };
    private readonly HudLabel _restartText = new()
    {
        Text = "Click, tap, Enter, or Space to restart",
        FontSize = 22f,
        Color = LightTextColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 425f,
    };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_titleText);
            Children.Add(_scoreText);
            Children.Add(_bestText);
            Children.Add(_restartText);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);

        _scoreText.Text = $"Score: {state.Score}";
        _bestText.Text = $"Best: {state.BestScore}";
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}