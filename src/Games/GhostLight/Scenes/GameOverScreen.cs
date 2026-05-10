using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.GhostLight.GhostLightConstants;

namespace SkiaSharpGames.GhostLight;

/// <summary>Game over overlay showing final survival time. Click to restart.</summary>
internal sealed class GameOverScreen(GhostLightState state, IDirector director) : Scene
{
    private readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha(180) };

    private readonly HudLabel _title = new()
    {
        Text = "CONSUMED BY DARKNESS",
        FontSize = 48f,
        Color = new SKColor(0xFF, 0x44, 0x66),
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 250f,
    };
    private readonly HudLabel _scoreText = new()
    {
        FontSize = 32f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 320f,
    };
    private readonly HudLabel _prompt = new()
    {
        Text = "Click or Tap to Play Again",
        FontSize = 24f,
        Color = new SKColor(0x66, 0xBB, 0xFF),
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 390f,
    };

    public override void OnActivating()
    {
        _scoreText.Text =
            $"Score: {(int)(state.TimeSurvived * 10)}  •  Survived: {state.TimeSurvived:F1}s";
        if (ChildCount == 0)
        {
            Children.Add(_title);
            Children.Add(_scoreText);
            Children.Add(_prompt);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        // Semi-transparent black overlay
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _overlayPaint);
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<StartScreen>(new FadeCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new FadeCurtain());
    }
}
