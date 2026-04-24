using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class VictoryScreen(SpaceInvadersGameState state, IDirector director) : Scene
{
    private static readonly SKPaint _overlayPaint = new()
    {
        Color = new SKColor(0x00, 0x1A, 0x10).WithAlpha((byte)(255 * 0.8f)),
        IsAntialias = true,
    };

    private readonly HudLabel _titleText = new()
    {
        Text = "WAVE CLEARED",
        FontSize = 72f,
        Color = AccentColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 250f,
    };
    private readonly HudLabel _scoreText = new()
    {
        Name = "score",
        FontSize = 32f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 320f,
    };
    private readonly HudLabel _restartText = new()
    {
        Text = "Click, tap, or press Space to defend again",
        FontSize = 24f,
        Color = HudDimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 410f,
    };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_titleText);
            Children.Add(_scoreText);
            Children.Add(_restartText);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);

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
