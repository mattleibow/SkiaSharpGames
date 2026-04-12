using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A rectangular sprite with rounded corners and an optional shine highlight on the top half.
/// Suitable for bricks, paddles, panels, buttons, and similar shapes.
/// </summary>
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
    }
}
