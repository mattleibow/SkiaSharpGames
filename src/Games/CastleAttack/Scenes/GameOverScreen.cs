using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>
/// Defeat scene drawn on top of the frozen play scene.
/// Does not clear the canvas — relies on the base play scene being drawn first.
/// </summary>
internal sealed class GameOverScreen(CastleAttackGameState state, IDirector director) : Scene
{
    private static readonly SKPaint OverlayPaint = new();

    private readonly HudLabel _defeatText = new()
    {
        Text = "DEFEAT",
        FontSize = 72f,
        Color = ColRed,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 250f,
    };
    private readonly HudLabel _scoreText = new()
    {
        FontSize = 32f,
        Color = ColHud,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 315f,
    };
    private readonly HudLabel _retryText = new()
    {
        Text = "Click or Tap to Try Again",
        FontSize = 22f,
        Color = ColAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 370f,
    };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_defeatText);
            Children.Add(_scoreText);
            Children.Add(_retryText);
        }
        _scoreText.Text = $"Score: {state.Score}";
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        OverlayPaint.Color = SKColors.Black.WithAlpha((byte)(255 * 0.75f));
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), OverlayPaint);
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}