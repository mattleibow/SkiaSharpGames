using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class StartScreen(IDirector director) : Scene
{
    private readonly HudLabel _title = new()
    {
        Text = "ASTEROIDS",
        FontSize = 72f,
        Color = AccentColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 195f,
    };
    private readonly HudLabel _subtitle = new()
    {
        Text = "Classic arcade space shooter",
        FontSize = 26f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 240f,
    };
    private readonly HudLabel _moveHint = new()
    {
        Text = "Rotate: Arrow keys or A/D",
        FontSize = 22f,
        Color = HudDimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 310f,
    };
    private readonly HudLabel _thrustHint = new()
    {
        Text = "Thrust: Up arrow or W",
        FontSize = 22f,
        Color = HudDimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 342f,
    };
    private readonly HudLabel _fireHint = new()
    {
        Text = "Fire: Space or Enter",
        FontSize = 22f,
        Color = HudDimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 374f,
    };
    private readonly HudLabel _goal = new()
    {
        Text = "Destroy all asteroids to survive",
        FontSize = 24f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 430f,
    };
    private readonly HudLabel _startPrompt = new()
    {
        Text = "Click, tap, or press Space to start",
        FontSize = 24f,
        Color = AccentColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 490f,
    };

    // Decorative asteroids drifting in the background
    private readonly List<(
        float x,
        float y,
        float vx,
        float vy,
        float r,
        SKPath path
    )> _bgAsteroids = [];
    private bool _initialized;

    public override void OnActivating()
    {
        if (!_initialized)
        {
            _initialized = true;
            var rng = new Random(42);
            for (int i = 0; i < 8; i++)
            {
                float x = rng.NextSingle() * GameWidth;
                float y = rng.NextSingle() * GameHeight;
                float angle = rng.NextSingle() * MathF.PI * 2f;
                float speed = 20f + rng.NextSingle() * 30f;
                float vx = MathF.Cos(angle) * speed;
                float vy = MathF.Sin(angle) * speed;
                float radius = 15f + rng.NextSingle() * 30f;
                var path = GenerateAsteroidPath(radius, rng.Next());
                _bgAsteroids.Add((x, y, vx, vy, radius, path));
            }
        }

        if (ChildCount == 0)
        {
            Children.Add(_title);
            Children.Add(_subtitle);
            Children.Add(_moveHint);
            Children.Add(_thrustHint);
            Children.Add(_fireHint);
            Children.Add(_goal);
            Children.Add(_startPrompt);
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        for (int i = 0; i < _bgAsteroids.Count; i++)
        {
            var a = _bgAsteroids[i];
            float x = a.x + a.vx * deltaTime;
            float y = a.y + a.vy * deltaTime;
            if (x < -a.r)
                x = GameWidth + a.r;
            else if (x > GameWidth + a.r)
                x = -a.r;
            if (y < -a.r)
                y = GameHeight + a.r;
            else if (y > GameHeight + a.r)
                y = -a.r;
            _bgAsteroids[i] = (x, y, a.vx, a.vy, a.r, a.path);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);

        // Draw background asteroids
        using var paint = new SKPaint
        {
            Color = AsteroidColor.WithAlpha(60),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
        };
        foreach (var a in _bgAsteroids)
        {
            canvas.Save();
            canvas.Translate(a.x, a.y);
            canvas.DrawPath(a.path, paint);
            canvas.Restore();
        }
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }

    private static SKPath GenerateAsteroidPath(float radius, int seed)
    {
        var rng = new Random(seed);
        int vertices = AsteroidVertices;
        var path = new SKPath();
        for (int i = 0; i < vertices; i++)
        {
            float angle = MathF.PI * 2f * i / vertices;
            float variation = 0.7f + rng.NextSingle() * 0.6f;
            float r = radius * variation;
            float px = MathF.Cos(angle) * r;
            float py = MathF.Sin(angle) * r;
            if (i == 0)
                path.MoveTo(px, py);
            else
                path.LineTo(px, py);
        }
        path.Close();
        return path;
    }
}
