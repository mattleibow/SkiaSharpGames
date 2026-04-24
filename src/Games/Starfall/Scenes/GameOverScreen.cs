using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Game over screen showing final score and restart option.
/// </summary>
internal sealed class GameOverScreen(StarfallGameState state, IDirector director) : Scene
{
    private readonly Starfield _starfield = new();
    private float _time;

    private readonly HudLabel _gameOverText = new()
    {
        Text = "GAME OVER",
        FontSize = 72f,
        Color = RedAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 200f,
    };
    private readonly HudLabel _scoreText = new()
    {
        FontSize = 34f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 280f,
    };
    private readonly HudLabel _stageText = new()
    {
        FontSize = 22f,
        Color = HudDimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 320f,
    };
    private readonly HudLabel _statsText = new()
    {
        FontSize = 18f,
        Color = HudDimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 360f,
    };
    private readonly HudLabel _restartText = new()
    {
        Text = "Click, tap, or press Space to try again",
        FontSize = 22f,
        Color = CyanAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 450f,
    };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_starfield);
            Children.Add(_gameOverText);
            Children.Add(_scoreText);
            Children.Add(_stageText);
            Children.Add(_statsText);
            Children.Add(_restartText);
        }

        _scoreText.Text = $"Score: {state.Score:N0}";
        string stageName = state.CurrentStage switch
        {
            1 => "Asteroid Belt",
            2 => "Pirate Territory",
            _ => "The Void",
        };
        _stageText.Text = $"Reached Sector {state.CurrentStage} — {stageName}";
        _statsText.Text = $"Score Multiplier: {state.ScoreMultiplier:F1}x  •  Bullet Damage: {state.BulletDamage}";
    }

    protected override void OnUpdate(float deltaTime)
    {
        _time += deltaTime;
        float pulse = 0.5f + 0.5f * MathF.Sin(_time * 2.5f);
        _restartText.Color = CyanAccent.WithAlpha((byte)(120 + 135 * pulse));
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);

        // Dim red overlay
        using var overlayPaint = new SKPaint { Color = RedAccent.WithAlpha(15) };
        canvas.DrawRect(0, 0, GameWidth, GameHeight, overlayPaint);
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<TitleScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<TitleScreen>(new DissolveCurtain());
    }
}
