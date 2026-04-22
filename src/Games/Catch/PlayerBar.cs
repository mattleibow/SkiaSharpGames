using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class PlayerBar : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPaint _shinePaint = new() { IsAntialias = true };

    public PlayerBar()
    {
        Collider = new RectCollider { Width = BarWidth, Height = BarHeight };
    }

    public new RectCollider Collider { get => (RectCollider)base.Collider!; init => base.Collider = value; }

    public float Width { get; set; } = BarWidth;
    public float Height { get; set; } = BarHeight;
    public SKColor Color { get; set; } = AccentColor;
    public float CornerRadius { get; set; } = 8f;
    public bool ShowShine { get; set; } = true;

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f)
            return;

        float left = -Width / 2f;
        float top = -Height / 2f;
        var rect = SKRect.Create(left, top, Width, Height);

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        canvas.DrawRoundRect(new SKRoundRect(rect, CornerRadius), _paint);

        if (ShowShine && Width > 4f && Height > 4f)
        {
            float shineHeight = (Height - 4f) / 2f;
            float shineRadius = Math.Max(CornerRadius - 1f, 0f);
            _shinePaint.Color = SKColors.White.WithAlpha((byte)(55 * Alpha));
            canvas.DrawRoundRect(
                new SKRoundRect(SKRect.Create(left + 2f, top + 2f, Width - 4f, shineHeight), shineRadius),
                _shinePaint);
        }
    }
}
