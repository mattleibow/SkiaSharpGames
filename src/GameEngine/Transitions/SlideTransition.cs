using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>Direction from which the incoming screen slides into view.</summary>
public enum SlideDirection { Up, Down, Left, Right }

/// <summary>
/// A slide transition: the incoming screen slides in from one edge, revealing it on top of the outgoing screen.
/// </summary>
public sealed class SlideTransition : IScreenTransition
{
    /// <summary>Edge from which the incoming screen enters. Defaults to <see cref="SlideDirection.Left"/>.</summary>
    public SlideDirection Direction { get; init; } = SlideDirection.Left;

    /// <inheritdoc />
    public float Duration { get; init; } = 0.4f;

    /// <inheritdoc />
    public void Draw(SKCanvas canvas, float progress,
                     Action<SKCanvas> drawOutgoing,
                     Action<SKCanvas> drawIncoming,
                     int width, int height)
    {
        // Outgoing screen is fully visible underneath
        drawOutgoing(canvas);

        // Incoming screen slides in from the specified edge
        float ease = Easing.EaseInOut(progress);

        float tx = 0f, ty = 0f;
        switch (Direction)
        {
            case SlideDirection.Left:  tx =  width  * (1f - ease); break;
            case SlideDirection.Right: tx = -width  * (1f - ease); break;
            case SlideDirection.Up:    ty =  height * (1f - ease); break;
            case SlideDirection.Down:  ty = -height * (1f - ease); break;
        }

        canvas.Save();
        canvas.Translate(tx, ty);
        drawIncoming(canvas);
        canvas.Restore();
    }
}
