using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Expanding shockwave ring from bombers. Damages player on contact.
/// </summary>
internal sealed class Shockwave : Actor
{
    private float _radius;
    private readonly float _maxRadius;

    public Shockwave(float x, float y, float maxRadius = ShockwaveMaxRadius)
    {
        X = x;
        Y = y;
        _maxRadius = maxRadius;
        Collider = new CircleCollider(1f);
    }

    public float CurrentRadius => _radius;

    protected override void OnUpdate(float deltaTime)
    {
        _radius += ShockwaveSpeed * deltaTime;
        if (Collider is CircleCollider cc)
            cc.Radius = _radius;

        Alpha = 1f - (_radius / _maxRadius);

        if (_radius >= _maxRadius)
            Active = false;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = ShockwaveColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3f,
        };
        canvas.DrawCircle(0, 0, _radius, paint);

        paint.StrokeWidth = 1f;
        paint.Color = OrangeAccent.WithAlpha(60);
        canvas.DrawCircle(0, 0, _radius * 0.7f, paint);
    }
}
