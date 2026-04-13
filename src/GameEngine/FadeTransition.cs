using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A transition that fades the screen to a solid colour and back.
/// </summary>
public sealed class FadeTransition : IScreenTransition
{
    /// <summary>The colour to fade to. Defaults to black.</summary>
    public SKColor Color { get; init; } = SKColors.Black;

    /// <inheritdoc />
    public void Draw(SKCanvas canvas, float coverage, (int width, int height) dimensions)
    {
        byte alpha = (byte)(coverage * 255);
        if (alpha == 0) return;

        using var paint = new SKPaint { Color = Color.WithAlpha(alpha) };
        canvas.DrawRect(SKRect.Create(0, 0, dimensions.width, dimensions.height), paint);
    }
}
