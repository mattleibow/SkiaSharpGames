using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.GhostLight;

/// <summary>Draws 4 large white fog circles with breathing alpha animation.</summary>
internal sealed class FogLayer : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private float _time;

    private static readonly (
        float x,
        float y,
        float radius,
        float baseAlpha,
        float offset
    )[] Circles =
    [
        (200f, 150f, 180f, 0.10f, 0f),
        (500f, 300f, 200f, 0.08f, 1.2f),
        (650f, 120f, 150f, 0.12f, 2.5f),
        (300f, 450f, 220f, 0.09f, 3.8f),
    ];

    public FogLayer()
    {
        Name = "fog";
    }

    protected override void OnUpdate(float deltaTime) => _time += deltaTime;

    protected override void OnDraw(SKCanvas canvas)
    {
        foreach (var (x, y, radius, baseAlpha, offset) in Circles)
        {
            float alpha = baseAlpha + 0.02f * MathF.Sin(_time + offset);
            _paint.Color = SKColors.White.WithAlpha((byte)(alpha * 255));
            canvas.DrawCircle(x, y, radius, _paint);
        }
    }
}
