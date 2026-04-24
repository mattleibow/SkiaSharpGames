using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

internal sealed class Brick : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPaint _shimmerPaint = new() { IsAntialias = true };

    public readonly int Row,
        Col;

    public float Width { get; set; } = BrickWidth;
    public float Height { get; set; } = BrickHeight;
    public SKColor Color { get; set; }
    public float CornerRadius { get; set; } = 3f;
    public LoopedAnimation Shimmer { get; } = new(period: 8f, duration: 0.8f);

    public Brick(int row, int col, float cx, float cy)
    {
        Row = row;
        Col = col;
        X = cx;
        Y = cy;
        Collider = new RectCollider { Width = BrickWidth, Height = BrickHeight };
    }

    public new RectCollider Collider
    {
        get => (RectCollider)base.Collider!;
        init => base.Collider = value;
    }

    protected override void OnUpdate(float deltaTime)
    {
        Shimmer.Update(deltaTime);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Alpha <= 0f)
            return;

        float left = 0 - Width / 2f;
        float top = 0 - Height / 2f;
        var rect = SKRect.Create(left, top, Width, Height);

        _paint.Color = Color.WithAlpha((byte)(255 * Alpha));
        canvas.DrawRoundRect(new SKRoundRect(rect, CornerRadius), _paint);

        if (Width > 4f && Height > 4f)
        {
            float shineHeight = (Height - 4f) / 2f;
            float shineRadius = Math.Max(CornerRadius - 1f, 0f);
            _paint.Color = SKColors.White.WithAlpha((byte)(55 * Alpha));
            canvas.DrawRoundRect(
                new SKRoundRect(
                    SKRect.Create(left + 2f, top + 2f, Width - 4f, shineHeight),
                    shineRadius
                ),
                _paint
            );
        }

        if (Shimmer.IsActive && Width > 0f && Height > 0f)
        {
            float stripeWidth = Width * 0.5f;
            float sweepX = left - stripeWidth / 2f + Shimmer.Progress * (Width + stripeWidth);

            canvas.Save();
            canvas.ClipRoundRect(new SKRoundRect(rect, CornerRadius));

            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(sweepX, top),
                new SKPoint(sweepX + stripeWidth, top),
                [
                    SKColors.Transparent,
                    SKColors.White.WithAlpha((byte)(90 * Alpha)),
                    SKColors.Transparent,
                ],
                [0f, 0.5f, 1f],
                SKShaderTileMode.Clamp
            );

            _shimmerPaint.Shader = shader;
            canvas.DrawRect(SKRect.Create(sweepX, top, stripeWidth, Height), _shimmerPaint);
            _shimmerPaint.Shader = null;
            canvas.Restore();
        }
    }
}