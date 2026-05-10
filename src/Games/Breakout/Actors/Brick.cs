using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

internal sealed class Brick : Actor
{
    private const float CornerRadius = 3f;
    private const float MinSize = 4f;
    private const float ShimmerPeriod = 8f;
    private const float ShimmerDuration = 0.8f;
    private const float ShimmerAlpha = 90f;

    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKPaint _shimmerPaint = new()
    {
        IsAntialias = true,
        Shader = CreateShimmerShader(BrickWidth),
    };
    private readonly LoopedAnimation _shimmer = new(
        period: ShimmerPeriod,
        duration: ShimmerDuration
    );

    public float Width
    {
        get;
        set
        {
            field = value;
            using var _ = _shimmerPaint.Shader;
            _shimmerPaint.Shader = CreateShimmerShader(value);
        }
    } = BrickWidth;

    public float Height { get; set; } = BrickHeight;

    public SKColor Color { get; set; }

    public int ScoreValue { get; set; }

    public Brick()
    {
        Collider = new RectCollider(BrickWidth, BrickHeight);
    }

    public void StartShimmer(float offset) => _shimmer.Start(offset);

    protected override void OnUpdate(float deltaTime) => _shimmer.Update(deltaTime);

    private static SKShader? CreateShimmerShader(float width) =>
        width <= MinSize
            ? null
            : SKShader.CreateLinearGradient(
                new SKPoint(0f, 0f),
                new SKPoint(width * 0.5f, 0f),
                [
                    SKColors.Transparent,
                    SKColors.White.WithAlpha((byte)ShimmerAlpha),
                    SKColors.Transparent,
                ],
                [0f, 0.5f, 1f],
                SKShaderTileMode.Clamp
            );

    protected override void OnDraw(SKCanvas canvas)
    {
        if (Width <= MinSize || Height <= MinSize)
            return;

        float left = 0 - Width / 2f;
        float top = 0 - Height / 2f;
        var rect = SKRect.Create(left, top, Width, Height);
        var rrect = new SKRoundRect(rect, CornerRadius);

        _paint.Color = Color;
        canvas.DrawRoundRect(rrect, _paint);

        float shineHeight = (Height - MinSize) / 2f;
        float shineRadius = Math.Max(CornerRadius - 1f, 0f);
        _paint.Color = SKColors.White.WithAlpha(55);
        canvas.DrawRoundRect(
            new SKRoundRect(
                SKRect.Create(left + 2f, top + 2f, Width - MinSize, shineHeight),
                shineRadius
            ),
            _paint
        );

        if (!_shimmer.IsActive)
            return;

        float stripeWidth = Width * 0.5f;
        float sweepX = left - stripeWidth / 2f + _shimmer.Progress * (Width + stripeWidth);

        canvas.Save();
        canvas.ClipRoundRect(rrect);
        canvas.Translate(sweepX, top);
        canvas.DrawRect(SKRect.Create(0f, 0f, stripeWidth, Height), _shimmerPaint);
        canvas.Restore();
    }
}
