using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Destructible asteroid obstacle. Drifts across the screen.
/// </summary>
internal sealed class StarfallAsteroid : EnemyBase
{
    private readonly SKPath _shape;
    private readonly float _rotSpeed;
    private float _rot;

    public StarfallAsteroid(float x, float y, float vx, float vy, bool large = true)
        : base(AsteroidHP, AsteroidScore, large ? AsteroidLargeRadius : AsteroidSmallRadius)
    {
        X = x;
        Y = y;
        Rigidbody = new Rigidbody2D();
        Rigidbody.SetVelocity(vx, vy);
        _rotSpeed = (Random.Shared.NextSingle() - 0.5f) * 4f;
        _shape = GenerateShape(Radius, Random.Shared.Next());
    }

    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        _rot += _rotSpeed * deltaTime;

        // Wrap horizontally
        if (X < -Radius * 2) X = GameWidth + Radius;
        if (X > GameWidth + Radius * 2) X = -Radius;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        var color = FlashColor(AsteroidColor);
        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        canvas.Save();
        canvas.RotateRadians(_rot);
        canvas.DrawPath(_shape, paint);

        // Outline
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1f;
        paint.Color = SKColors.White.WithAlpha(60);
        canvas.DrawPath(_shape, paint);
        canvas.Restore();
    }

    private static SKPath GenerateShape(float radius, int seed)
    {
        var rng = new Random(seed);
        int vertices = 8;
        var path = new SKPath();
        for (int i = 0; i < vertices; i++)
        {
            float angle = MathF.PI * 2f * i / vertices;
            float variation = 0.7f + rng.NextSingle() * 0.6f;
            float r = radius * variation;
            float px = MathF.Cos(angle) * r;
            float py = MathF.Sin(angle) * r;
            if (i == 0) path.MoveTo(px, py);
            else path.LineTo(px, py);
        }
        path.Close();
        return path;
    }
}
