using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A rectangular sprite with rounded corners and an optional shine highlight on the top half.
/// Suitable for bricks, paddles, panels, buttons, and similar shapes.
/// </summary>
/// <remarks>
/// The sprite exposes a <see cref="Shimmer"/> <see cref="LoopedAnimation"/> that sweeps a bright
/// gradient stripe across the rectangle periodically. Call <see cref="Shimmer"/>.Start() to enable it,
/// and <see cref="Update"/> every game tick to advance the animation.
/// </remarks>
public class RectSprite : Sprite
{
    /// <summary>Width of the rectangle in game-space units.</summary>
    public float Width { get; set; }

    /// <summary>Height of the rectangle in game-space units.</summary>
    public float Height { get; set; }

    /// <summary>Fill colour.</summary>
    public SKColor Color { get; set; }

    /// <summary>Corner radius. Defaults to 4.</summary>
    public float CornerRadius { get; set; } = 4f;

    /// <summary>
    /// When true a semi-transparent white highlight is painted on the top half of the rect
    /// to give a subtle bevel/glossy look.
    /// </summary>
    public bool ShowShine { get; set; } = true;

    /// <summary>
    /// A looping shimmer animation that sweeps a white gradient stripe across the rectangle.
    /// By default the period is 8 s and each run lasts 0.8 s.
    /// Call <see cref="LoopedAnimation.Start"/> to enable, and <see cref="Update"/> every tick
    /// to advance it.
    /// </summary>
    public LoopedAnimation Shimmer { get; } = new LoopedAnimation(period: 8f, duration: 0.8f);

    /// <inheritdoc />
    public override void Update(float deltaTime) => Shimmer.Update(deltaTime);

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas)
    {
        if (!Visible || Alpha <= 0f) return;

        byte a = (byte)(255 * Alpha);
        var rect = SKRect.Create(X, Y, Width, Height);

        using var fill = new SKPaint { Color = Color.WithAlpha(a), IsAntialias = true };
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, fill);

        if (ShowShine && Width > 4f && Height > 4f)
        {
            var shineRect = SKRect.Create(X + 2f, Y + 2f, Width - 4f, (Height - 4f) / 2f);
            float cr = Math.Max(CornerRadius - 1f, 0f);
            using var shine = new SKPaint
            {
                Color = SKColors.White.WithAlpha((byte)(55 * Alpha)),
                IsAntialias = true
            };
            canvas.DrawRoundRect(shineRect, cr, cr, shine);
        }

        // Shimmer — a white gradient stripe sweeping left→right
        if (Shimmer.IsActive && Width > 0f && Height > 0f)
        {
            float stripeW = Width * 0.5f;
            // Centre of the stripe travels from X-stripeW/2 to X+Width+stripeW/2
            float sweepX = X - stripeW / 2f + Shimmer.Progress * (Width + stripeW);

            canvas.Save();
            canvas.ClipRoundRect(new SKRoundRect(rect, CornerRadius));

            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(sweepX, Y),
                new SKPoint(sweepX + stripeW, Y),
                [
                    SKColors.Transparent,
                    SKColors.White.WithAlpha((byte)(90 * Alpha)),
                    SKColors.Transparent
                ],
                [0f, 0.5f, 1f],
                SKShaderTileMode.Clamp);

            using var shimmerPaint = new SKPaint { Shader = shader, IsAntialias = true };
            canvas.DrawRect(SKRect.Create(sweepX, Y, stripeW, Height), shimmerPaint);

            canvas.Restore();
        }
    }
}
