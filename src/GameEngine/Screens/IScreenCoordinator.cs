namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Manages the active screen stack and cross-screen transitions for a game.
/// A screen can call methods on this coordinator (available via <see cref="GameScreenBase.Coordinator"/>)
/// to switch to a new screen, push an overlay, or pop the top overlay.
/// </summary>
public interface IScreenCoordinator
{
    /// <summary>
    /// Replaces the current screen with a new instance of <typeparamref name="TScreen"/>,
    /// playing <paramref name="transition"/> between them. Clears any overlays first.
    /// </summary>
    /// <typeparam name="TScreen">
    /// Screen type to transition to. Must be registered in the game's service container.
    /// </typeparam>
    /// <param name="transition">
    /// Optional transition effect. Pass <see langword="null"/> for an instant switch.
    /// </param>
    void TransitionTo<TScreen>(IScreenTransition? transition = null) where TScreen : GameScreenBase;

    /// <summary>
    /// Pushes a new overlay screen on top of the current screen.
    /// The current screen is paused (still drawn, not updated) while any overlay is showing.
    /// </summary>
    /// <typeparam name="TOverlay">
    /// Overlay screen type. Must be registered in the game's service container.
    /// </typeparam>
    void PushOverlay<TOverlay>() where TOverlay : GameScreenBase;

    /// <summary>
    /// Removes the top overlay and resumes the underlying screen.
    /// Does nothing if no overlay is currently active.
    /// </summary>
    void PopOverlay();
}
