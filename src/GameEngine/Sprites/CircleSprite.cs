using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A circular sprite with an optional glow halo rendered via a blur mask filter.
/// Suitable for balls, particles, indicators, and similar round objects.
/// </summary>
public class CircleSprite : Sprite
{
    /// <summary>Radius of the circle in game-space units.</summary>
    public float Radius { get; set; }

    /// <summary>Fill colour of the circle.</summary>
    public SKColor Color { get; set; }

    /// <summary>
    /// Sigma (blur radius) of the glow effect in game-space units.
    /// Set to 0 to disable the glow.
    /// </summary>
    public float GlowRadius { get; set; } = 0f;

    /// <summary>Colour of the glow halo. Defaults to white.</summary>
    public SKColor GlowColor { get; set; } = SKColors.White;

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible || Alpha <= 0f) return;

        byte a = (byte)(255 * Alpha);

        if (GlowRadius > 0f)
        {
            using var glow = new SKPaint
            {
                Color = GlowColor.WithAlpha((byte)(60 * Alpha)),
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, GlowRadius)
            };
            canvas.DrawCircle(x, y, Radius + GlowRadius / 2f, glow);
        }

        using var fill = new SKPaint { Color = Color.WithAlpha(a), IsAntialias = true };
        canvas.DrawCircle(x, y, Radius, fill);
    }
}
