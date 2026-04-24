namespace SkiaSharp.Theatre;

/// <summary>
/// A transition that fades the outgoing scene to a solid colour and then reveals the incoming scene.
/// The total duration is split evenly: first half fades out, second half fades in.
/// </summary>
public sealed class FadeCurtain : ICurtain
{
    private readonly SKPaint _overlayPaint = new() { IsAntialias = true };

    /// <summary>The colour to fade through. Defaults to black.</summary>
    public SKColor Color { get; init; } = SKColors.Black;

    /// <inheritdoc />
    public float Duration { get; init; } = 0.7f;

    /// <inheritdoc />
    public void Draw(
        SKCanvas canvas,
        float progress,
        Action<SKCanvas> drawOutgoing,
        Action<SKCanvas> drawIncoming,
        int width,
        int height
    )
    {
        float overlayAlpha;

        if (progress < 0.5f)
        {
            // First half: outgoing scene fades toward solid colour
            drawOutgoing(canvas);
            overlayAlpha = progress * 2f; // 0 → 1
        }
        else
        {
            // Second half: incoming scene reveals from solid colour
            drawIncoming(canvas);
            overlayAlpha = 1f - (progress - 0.5f) * 2f; // 1 → 0
        }

        if (overlayAlpha > 0f)
        {
            byte a = (byte)(255 * Math.Clamp(overlayAlpha, 0f, 1f));
            _overlayPaint.Color = Color.WithAlpha(a);
            canvas.DrawRect(SKRect.Create(0, 0, width, height), _overlayPaint);
        }
    }
}