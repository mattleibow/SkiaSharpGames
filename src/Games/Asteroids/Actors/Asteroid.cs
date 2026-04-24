using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal enum AsteroidSize
{
    Large,
    Medium,
    Small,
}

internal sealed class Asteroid : Actor
{
    private static readonly SKPaint _strokePaint = new()
    {
        Color = AsteroidColor,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.5f,
    };

    private readonly SKPath _shapePath;

    public AsteroidSize Size { get; }
    public float RotationSpeed { get; }

    public Asteroid(AsteroidSize size, float x, float y, float vx, float vy, int seed)
    {
        Size = size;
        X = x;
        Y = y;

        float radius = size switch
        {
            AsteroidSize.Large => AsteroidLargeRadius,
            AsteroidSize.Medium => AsteroidMediumRadius,
            _ => AsteroidSmallRadius,
        };

        Collider = new CircleCollider { Radius = radius * 0.8f };
        Rigidbody = new Rigidbody2D { VelocityX = vx, VelocityY = vy };
        _shapePath = GenerateShape(radius, seed);
        RotationSpeed = (Random.Shared.NextSingle() - 0.5f) * 3f;
    }

    public int ScoreValue =>
        Size switch
        {
            AsteroidSize.Large => AsteroidLargeScore,
            AsteroidSize.Medium => AsteroidMediumScore,
            _ => AsteroidSmallScore,
        };

    protected override void OnUpdate(float deltaTime)
    {
        Rotation += RotationSpeed * deltaTime;

        // Screen wrap
        float radius = Size switch
        {
            AsteroidSize.Large => AsteroidLargeRadius,
            AsteroidSize.Medium => AsteroidMediumRadius,
            _ => AsteroidSmallRadius,
        };

        if (X < -radius)
            X = GameWidth + radius;
        else if (X > GameWidth + radius)
            X = -radius;
        if (Y < -radius)
            Y = GameHeight + radius;
        else if (Y > GameHeight + radius)
            Y = -radius;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawPath(_shapePath, _strokePaint);
    }

    private static SKPath GenerateShape(float radius, int seed)
    {
        var random = new Random(seed);
        int vertices = AsteroidVertices;
        var path = new SKPath();

        for (int i = 0; i < vertices; i++)
        {
            float angle = MathF.PI * 2f * i / vertices;
            float variation = 0.7f + random.NextSingle() * 0.6f;
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
