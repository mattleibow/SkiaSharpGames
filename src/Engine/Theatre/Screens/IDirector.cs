namespace SkiaSharp.Theatre;

/// <summary>
/// Manages the active screen stack and transitions between screens.
/// </summary>
/// <remarks>
/// Inject this interface into a <see cref="Scene"/> constructor to trigger transitions,
/// push overlays, and pop overlays.
/// </remarks>
public interface IDirector
{
    /// <summary>
    /// Replaces the current screen with a new <typeparamref name="TScreen"/> instance,
    /// optionally playing a cross-screen transition. Clears any open overlays first.
    /// </summary>
    void TransitionTo<TScreen>(ICurtain? transition = null)
        where TScreen : Scene;

    /// <summary>
    /// Pushes an overlay screen on top of the current screen.
    /// The current screen is paused (still drawn, not updated) while any overlay is active.
    /// </summary>
    void PushOverlay<TOverlay>() where TOverlay : Scene;

    /// <summary>
    /// Removes the topmost overlay and resumes the underlying screen.
    /// Does nothing if no overlay is currently active.
    /// </summary>
    void PopOverlay();

    /// <summary>The screen that should receive input events.</summary>
    Scene ActiveInputScene { get; }
}
