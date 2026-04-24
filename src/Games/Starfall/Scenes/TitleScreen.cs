using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Title screen with animated starfield, game title, and start prompt.
/// </summary>
internal sealed class TitleScreen(StarfallGameState state, IDirector director) : Scene
{
    private readonly Starfield _starfield = new();
    private float _time;
    private bool _initialized;

    private readonly HudLabel _title = new()
    {
        Text = "STARFALL",
        FontSize = 80f,
        Color = CyanAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 200f,
    };
    private readonly HudLabel _subtitle = new()
    {
        Text = "A Space Survival Shooter",
        FontSize = 24f,
        Color = WhiteColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 245f,
    };
    private readonly HudLabel _controlHint1 = new()
    {
        Text = "Mouse/Touch to move  •  Auto-fire",
        FontSize = 20f,
        Color = HudDimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 320f,
    };
    private readonly HudLabel _controlHint2 = new()
    {
        Text = "Space/Tap bomb button for special weapon",
        FontSize = 20f,
        Color = HudDimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 350f,
    };
    private readonly HudLabel _objective = new()
    {
        Text = "Survive 3 sectors. Defeat the bosses.",
        FontSize = 22f,
        Color = MagentaAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 410f,
    };
    private readonly HudLabel _startPrompt = new()
    {
        Text = "Click, tap, or press Space to launch",
        FontSize = 24f,
        Color = CyanAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 490f,
    };

    // Decorative floating enemies
    private readonly List<(float x, float y, float vx, float vy, float rot, int type)> _bgEntities = [];

    public override void OnActivating()
    {
        if (!_initialized)
        {
            _initialized = true;
            var rng = new Random(77);
            for (int i = 0; i < 6; i++)
            {
                float x = rng.NextSingle() * GameWidth;
                float y = rng.NextSingle() * GameHeight;
                float vx = (rng.NextSingle() - 0.5f) * 40f;
                float vy = 20f + rng.NextSingle() * 30f;
                _bgEntities.Add((x, y, vx, vy, rng.NextSingle() * MathF.PI * 2f, rng.Next(3)));
            }
        }

        if (ChildCount == 0)
        {
            Children.Add(_starfield);
            Children.Add(_title);
            Children.Add(_subtitle);
            Children.Add(_controlHint1);
            Children.Add(_controlHint2);
            Children.Add(_objective);
            Children.Add(_startPrompt);
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        _time += deltaTime;

        // Pulse start prompt
        float pulse = 0.5f + 0.5f * MathF.Sin(_time * 3f);
        _startPrompt.Color = CyanAccent.WithAlpha((byte)(120 + 135 * pulse));

        // Move background entities
        for (int i = 0; i < _bgEntities.Count; i++)
        {
            var e = _bgEntities[i];
            float x = e.x + e.vx * deltaTime;
            float y = e.y + e.vy * deltaTime;
            float rot = e.rot + deltaTime * 0.5f;
            if (y > GameHeight + 30f) y = -30f;
            if (x < -30f) x = GameWidth + 30f;
            if (x > GameWidth + 30f) x = -30f;
            _bgEntities[i] = (x, y, e.vx, e.vy, rot, e.type);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);

        // Draw background entities (decorative)
        using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        foreach (var e in _bgEntities)
        {
            canvas.Save();
            canvas.Translate(e.x, e.y);
            canvas.RotateRadians(e.rot);

            paint.Color = e.type switch
            {
                0 => DroneColor.WithAlpha(50),
                1 => ZigzaggerColor.WithAlpha(50),
                _ => AsteroidColor.WithAlpha(50),
            };

            float r = 12f;
            canvas.DrawCircle(0, 0, r, paint);
            canvas.Restore();
        }

        // Title glow effect
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 12f),
            Color = CyanAccent.WithAlpha(30),
        };
        var font = new SKFont { Size = 80f };
        float textW = font.MeasureText("STARFALL");
        canvas.DrawText("STARFALL", (GameWidth - textW) / 2f, 200f, font, glowPaint);
    }

    public override void OnPointerDown(float x, float y) => StartGame();

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            StartGame();
    }

    private void StartGame()
    {
        state.Reset();
        director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
