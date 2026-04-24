using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Victory screen shown after defeating the final boss.
/// </summary>
internal sealed class VictoryScreen(StarfallGameState state, IDirector director) : Scene
{
    private readonly Starfield _starfield = new();
    private float _time;
    private float _burstTimer = 3f;

    // Celebration particles
    private readonly List<(float x, float y, float vx, float vy, SKColor color, float life)> _confetti = [];

    private readonly HudLabel _victoryText = new()
    {
        Text = "VICTORY!",
        FontSize = 80f,
        Color = GreenAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 180f,
    };
    private readonly HudLabel _subtitleText = new()
    {
        Text = "The void has been purged",
        FontSize = 26f,
        Color = CyanAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 225f,
    };
    private readonly HudLabel _scoreText = new()
    {
        FontSize = 36f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 310f,
    };
    private readonly HudLabel _statsText = new()
    {
        FontSize = 18f,
        Color = HudDimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 355f,
    };
    private readonly HudLabel _upgradesText = new()
    {
        FontSize = 16f,
        Color = MagentaAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 385f,
    };
    private readonly HudLabel _restartText = new()
    {
        Text = "Click, tap, or press Space to play again",
        FontSize = 22f,
        Color = CyanAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 470f,
    };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_starfield);
            Children.Add(_victoryText);
            Children.Add(_subtitleText);
            Children.Add(_scoreText);
            Children.Add(_statsText);
            Children.Add(_upgradesText);
            Children.Add(_restartText);
        }

        _scoreText.Text = $"Final Score: {state.Score:N0}";
        _statsText.Text = $"Multiplier: {state.ScoreMultiplier:F1}x  •  Damage: {state.BulletDamage}  •  HP: {state.MaxHP}";
        _upgradesText.Text = $"Fire Rate: {state.FireRateMultiplier:F2}x  •  Speed: {state.SpeedMultiplier:F2}x  •  Bombs: {state.MaxBombs}";

        // Initial confetti burst
        _confetti.Clear();
        SpawnConfettiBurst();
    }

    protected override void OnUpdate(float deltaTime)
    {
        _time += deltaTime;

        // Pulse text
        float pulse = 0.6f + 0.4f * MathF.Sin(_time * 2f);
        _victoryText.Color = GreenAccent.WithAlpha((byte)(180 + 75 * pulse));

        float restartPulse = 0.5f + 0.5f * MathF.Sin(_time * 3f);
        _restartText.Color = CyanAccent.WithAlpha((byte)(120 + 135 * restartPulse));

        // Periodically spawn more confetti
        _burstTimer -= deltaTime;
        if (_burstTimer <= 0)
        {
            _burstTimer = 3f;
            SpawnConfettiBurst();
        }

        // Update confetti
        for (int i = _confetti.Count - 1; i >= 0; i--)
        {
            var c = _confetti[i];
            float nx = c.x + c.vx * deltaTime;
            float ny = c.y + c.vy * deltaTime;
            float nvy = c.vy + 80f * deltaTime; // gravity
            float nlife = c.life - deltaTime;
            if (nlife <= 0f)
                _confetti.RemoveAt(i);
            else
                _confetti[i] = (nx, ny, c.vx, nvy, c.color, nlife);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);

        // Subtle green tint overlay
        using var overlayPaint = new SKPaint { Color = GreenAccent.WithAlpha(10) };
        canvas.DrawRect(0, 0, GameWidth, GameHeight, overlayPaint);

        // Confetti
        using var confettiPaint = new SKPaint { IsAntialias = true };
        foreach (var c in _confetti)
        {
            float alpha = Math.Min(1f, c.life);
            confettiPaint.Color = c.color.WithAlpha((byte)(200 * alpha));
            canvas.DrawRect(c.x - 3f, c.y - 3f, 6f, 6f, confettiPaint);
        }

        // Victory title glow
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15f),
            Color = GreenAccent.WithAlpha(25),
        };
        var font = new SKFont { Size = 80f };
        float tw = font.MeasureText("VICTORY!");
        canvas.DrawText("VICTORY!", (GameWidth - tw) / 2f, 180f, font, glowPaint);
    }

    private void SpawnConfettiBurst()
    {
        SKColor[] colors = [CyanAccent, MagentaAccent, GreenAccent, YellowAccent, OrangeAccent];
        for (int i = 0; i < 20; i++)
        {
            float x = Random.Shared.NextSingle() * GameWidth;
            float y = -10f;
            float vx = (Random.Shared.NextSingle() - 0.5f) * 200f;
            float vy = Random.Shared.NextSingle() * 100f + 50f;
            var color = colors[Random.Shared.Next(colors.Length)];
            float life = 2f + Random.Shared.NextSingle() * 2f;
            _confetti.Add((x, y, vx, vy, color, life));
        }
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<TitleScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<TitleScreen>(new DissolveCurtain());
    }
}
