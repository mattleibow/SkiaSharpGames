using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class Mine : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPaint _glowPaint = new() { IsAntialias = true };
    private float _cachedGlowRadius = float.NaN;

    private const float Radius = MineRadius;
    private static readonly SKColor Color = new(0xD9, 0x78, 0x24);
    private const float GlowRadius = 4f;
    private static readonly SKColor GlowColor = new(0xFF, 0xBF, 0x66);

    public Mine()
    {
        Collider = new CircleCollider { Radius = MineRadius };
        Rigidbody = new Rigidbody2D();
    }

    public new CircleCollider Collider
    {
        get => (CircleCollider)base.Collider!;
        init => base.Collider = value;
    }
    public new Rigidbody2D Rigidbody
    {
        get => (Rigidbody2D)base.Rigidbody!;
        init => base.Rigidbody = value;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f)
            return;

        if (GlowRadius > 0f)
        {
            EnsureGlowMask();
            _glowPaint.Color = GlowColor.WithAlpha((byte)(55 * Alpha));
            canvas.DrawCircle(0f, 0f, Radius + GlowRadius / 2f, _glowPaint);
        }

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        canvas.DrawCircle(0f, 0f, Radius, _paint);
    }

    private void EnsureGlowMask()
    {
        if (GlowRadius.Equals(_cachedGlowRadius))
            return;

        _glowPaint.MaskFilter?.Dispose();
        _glowPaint.MaskFilter =
            GlowRadius > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowRadius) : null;
        _cachedGlowRadius = GlowRadius;
    }
}
