using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A cross-dissolve transition: the outgoing screen fades out while the incoming screen fades in.
/// At any point during the transition both screens are visible, blended by <c>progress</c>.
/// </summary>
public sealed class DissolveTransition : IScreenTransition
{
    /// <inheritdoc />
    public float Duration { get; init; } = 0.4f;

    /// <inheritdoc />
    public void Draw(SKCanvas canvas, float progress,
                     Action<SKCanvas> drawOutgoing,
                     Action<SKCanvas> drawIncoming,
                     int width, int height)
    {
        // Draw outgoing at (1 – progress) opacity
        using var outPaint = new SKPaint
        {
            Color = SKColors.White.WithAlpha((byte)((1f - progress) * 255))
        };
        canvas.SaveLayer(outPaint);
        drawOutgoing(canvas);
        canvas.Restore();

        // Draw incoming at progress opacity on top
        using var inPaint = new SKPaint
        {
            Color = SKColors.White.WithAlpha((byte)(progress * 255))
        };
        canvas.SaveLayer(inPaint);
        drawIncoming(canvas);
        canvas.Restore();
    }
}
