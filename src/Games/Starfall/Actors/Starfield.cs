using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Multi-layer parallax starfield background rendered as an actor.
/// Three layers of stars move at different speeds for depth effect.
/// </summary>
internal sealed class Starfield : Actor
{
    private readonly List<(float x, float y, float size, byte alpha, int layer)> _stars = [];

    public Starfield()
    {
        Name = "starfield";
        var rng = new Random(42);
        for (int i = 0; i < 120; i++)
        {
            float x = rng.NextSingle() * GameWidth;
            float y = rng.NextSingle() * GameHeight;
            int layer = rng.Next(3); // 0=far, 1=mid, 2=near
            float size = layer switch
            {
                0 => 1f,
                1 => 1.5f,
                _ => 2.5f,
            };
            byte alpha = layer switch
            {
                0 => 40,
                1 => 80,
                _ => 160,
            };
            _stars.Add((x, y, size, alpha, layer));
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        for (int i = 0; i < _stars.Count; i++)
        {
            var s = _stars[i];
            float speed = s.layer switch
            {
                0 => 15f,
                1 => 35f,
                _ => 60f,
            };
            float y = s.y + speed * deltaTime;
            if (y > GameHeight + 4f)
                y = -4f;
            _stars[i] = (s.x, y, s.size, s.alpha, s.layer);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        using var paint = new SKPaint { IsAntialias = true };
        foreach (var s in _stars)
        {
            paint.Color = SKColors.White.WithAlpha(s.alpha);
            canvas.DrawCircle(s.x, s.y, s.size * 0.5f, paint);
        }
    }
}
