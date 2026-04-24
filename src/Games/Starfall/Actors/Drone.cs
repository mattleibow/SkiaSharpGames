using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Basic enemy that flies straight down.
/// </summary>
internal sealed class Drone : EnemyBase
{
    private static readonly SKPaint _paint = new() { IsAntialias = true };
    private static readonly SKPaint _corePaint = new()
    {
        Color = SKColors.White.WithAlpha(180),
        IsAntialias = true,
    };
    private static readonly SKPath _shape;

    static Drone()
    {
        _shape = new SKPath();
        _shape.MoveTo(0, -DroneRadius);
        _shape.LineTo(-DroneRadius * 0.7f, 0);
        _shape.LineTo(0, DroneRadius * 0.6f);
        _shape.LineTo(DroneRadius * 0.7f, 0);
        _shape.Close();
    }

    private readonly float _speed;

    public Drone(float x, float y, float speed = DroneSpeed)
        : base(DroneHP, DroneScore, DroneRadius)
    {
        X = x;
        Y = y;
        _speed = speed;
        Rigidbody = new Rigidbody2D();
        Rigidbody.SetVelocity(0, _speed);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        _paint.Color = FlashColor(DroneColor);
        canvas.DrawPath(_shape, _paint);
        canvas.DrawCircle(0, 0, 3f, _corePaint);
    }
}
