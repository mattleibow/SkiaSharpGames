using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Abstract base class for all game screens (start, play, game-over, overlays, …).
/// Each screen is responsible for updating its own state and drawing itself.
/// </summary>
/// <remarks>
/// The screen coordinator injects itself via <see cref="SetCoordinator"/> before calling
/// <see cref="OnActivated"/>. Use the protected <see cref="Coordinator"/> property to trigger
/// transitions from within a screen.
/// </remarks>
public abstract class GameScreenBase
{
    /// <summary>
    /// Gets the logical (virtual) dimensions of the game canvas in game-space units.
    /// The default is 800 × 600. Override in subclasses to change.
    /// </summary>
    public virtual (int width, int height) GameDimensions => (800, 600);

    /// <summary>
    /// True while this screen has an overlay on top of it and is therefore not being updated.
    /// The screen is still drawn as the base layer while paused.
    /// </summary>
    public bool IsPaused { get; internal set; }

    /// <summary>The coordinator that manages this screen. Available after <see cref="OnActivated"/>.</summary>
    protected IScreenCoordinator? Coordinator { get; private set; }

    /// <summary>Called by <see cref="ScreenCoordinator"/> to inject itself before activation.</summary>
    internal void SetCoordinator(IScreenCoordinator coordinator) => Coordinator = coordinator;

    // ── Abstract contract ─────────────────────────────────────────────────

    /// <summary>Called each game tick to update the game state. Not called while <see cref="IsPaused"/>.</summary>
    /// <param name="deltaTime">Seconds elapsed since the last update.</param>
    public abstract void Update(float deltaTime);

    /// <summary>Called each frame to render the current state to <paramref name="canvas"/>.</summary>
    /// <param name="canvas">The SkiaSharp canvas to draw on.</param>
    /// <param name="width">Render-target width in pixels.</param>
    /// <param name="height">Render-target height in pixels.</param>
    public abstract void Draw(SKCanvas canvas, int width, int height);

    // ── Input ─────────────────────────────────────────────────────────────

    /// <summary>Called when the pointer/mouse moves over the canvas.</summary>
    public virtual void OnPointerMove(float x, float y) { }

    /// <summary>Called when the user clicks or taps the canvas.</summary>
    public virtual void OnPointerDown(float x, float y) { }

    /// <summary>Called when the user releases a click or touch on the canvas.</summary>
    public virtual void OnPointerUp(float x, float y) { }

    /// <summary>Called when a key is pressed while the game canvas has focus.</summary>
    public virtual void OnKeyDown(string key) { }

    /// <summary>Called when a key is released while the game canvas has focus.</summary>
    public virtual void OnKeyUp(string key) { }

    // ── Lifecycle ─────────────────────────────────────────────────────────

    /// <summary>Called once when this screen becomes the active (or initial) screen.</summary>
    public virtual void OnActivated() { }

    /// <summary>Called once when this screen is replaced by a transition or removed entirely.</summary>
    public virtual void OnDeactivated() { }

    /// <summary>Called when an overlay is pushed on top of this screen.</summary>
    public virtual void OnPaused() { }

    /// <summary>Called when the last overlay on top of this screen is removed.</summary>
    public virtual void OnResumed() { }
}
