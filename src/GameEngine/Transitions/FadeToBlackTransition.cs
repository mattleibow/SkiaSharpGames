using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A transition that fades the outgoing screen to a solid colour and then reveals the incoming screen.
/// The total duration is split evenly: first half fades out, second half fades in.
/// </summary>
public sealed class FadeToBlackTransition : IScreenTransition
{
    /// <summary>The colour to fade through. Defaults to black.</summary>
    public SKColor Color { get; init; } = SKColors.Black;

    /// <inheritdoc />
    public float Duration { get; init; } = 0.7f;

    /// <inheritdoc />
    public void Draw(SKCanvas canvas, float progress,
                     Action<SKCanvas> drawOutgoing,
                     Action<SKCanvas> drawIncoming,
                     int width, int height)
    {
        float overlayAlpha;

        if (progress < 0.5f)
        {
            // First half: outgoing screen fades toward solid colour
            drawOutgoing(canvas);
            overlayAlpha = progress * 2f; // 0 → 1
        }
        else
        {
            // Second half: incoming screen reveals from solid colour
            drawIncoming(canvas);
            overlayAlpha = 1f - (progress - 0.5f) * 2f; // 1 → 0
        }

        if (overlayAlpha > 0f)
        {
            byte a = (byte)(overlayAlpha * 255);
            using var paint = new SKPaint { Color = Color.WithAlpha(a) };
            canvas.DrawRect(SKRect.Create(0, 0, width, height), paint);
        }
    }
}
