namespace SkiaSharp.Theatre;

/// <summary>Direction from which the incoming scene slides into view.</summary>
public enum SlideDirection
{
    Up,
    Down,
    Left,
    Right,
}

/// <summary>
/// A slide transition: the incoming scene slides in from one edge, revealing it on top of the outgoing scene.
/// </summary>
public sealed class SlideCurtain : ICurtain
{
    /// <summary>
    /// Edge from which the incoming scene enters. Defaults to <see cref="SlideDirection.Left"/>.
    /// </summary>
    public SlideDirection Direction { get; init; } = SlideDirection.Left;

    /// <inheritdoc />
    public float Duration { get; init; } = 0.4f;

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
        // Outgoing scene is fully visible underneath
        drawOutgoing(canvas);

        // Incoming scene slides in from the specified edge
        float ease = Easing.EaseInOut(progress);

        float tx = 0f,
            ty = 0f;
        switch (Direction)
        {
            case SlideDirection.Left:
                tx = width * (1f - ease);
                break;
            case SlideDirection.Right:
                tx = -width * (1f - ease);
                break;
            case SlideDirection.Up:
                ty = height * (1f - ease);
                break;
            case SlideDirection.Down:
                ty = -height * (1f - ease);
                break;
        }

        canvas.Save();
        canvas.Translate(tx, ty);
        drawIncoming(canvas);
        canvas.Restore();
    }
}