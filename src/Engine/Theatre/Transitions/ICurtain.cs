using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Defines a visual transition between two game screens.
/// Implement this to create custom transition effects such as dissolves, slides, or wipes.
/// </summary>
public interface ICurtain
{
    /// <summary>Total duration of the transition in seconds.</summary>
    float Duration { get; }

    /// <summary>
    /// Renders the transition frame at the given <paramref name="progress"/>.
    /// </summary>
    /// <param name="canvas">Canvas to draw on. Already transformed to game-space coordinates.</param>
    /// <param name="progress">Normalised time: 0 = start (outgoing fully visible), 1 = end (incoming fully visible).</param>
    /// <param name="drawOutgoing">Callback that draws the outgoing screen onto the supplied canvas.</param>
    /// <param name="drawIncoming">Callback that draws the incoming screen onto the supplied canvas.</param>
    /// <param name="width">Stage-space width in game units.</param>
    /// <param name="height">Stage-space height in game units.</param>
    void Draw(SKCanvas canvas, float progress,
              Action<SKCanvas> drawOutgoing,
              Action<SKCanvas> drawIncoming,
              int width, int height);
}
