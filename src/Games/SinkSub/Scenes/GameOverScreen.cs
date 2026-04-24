using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class GameOverScreen(SinkSubGameState state, IDirector director) : Scene
{
    private static readonly SKPaint _fillPaint = new() { IsAntialias = true };

    private readonly HudLabel _titleText = new()
    {
        Text = "SHIP SUNK",
        FontSize = 62f,
        Color = new SKColor(0xFF, 0x73, 0x5A),
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 250f,
    };
    private readonly HudLabel _scoreText = new()
    {
        FontSize = 30f,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 320f,
    };
    private readonly HudLabel _waveText = new()
    {
        FontSize = 24f,
        Color = AccentColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 360f,
    };
    private readonly HudLabel _restartText = new()
    {
        Text = "Click, tap, or press Space to sail again",
        FontSize = 22f,
        Color = DimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 420f,
    };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_titleText);
            Children.Add(_scoreText);
            Children.Add(_waveText);
            Children.Add(_restartText);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        _fillPaint.Color = SKColors.Black.WithAlpha((byte)(255 * 0.78f));
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _fillPaint);

        _scoreText.Text = $"Score: {state.Score}";
        _waveText.Text = $"Wave reached: {state.Wave}";
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
