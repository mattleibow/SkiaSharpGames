using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Abstract base class for all game screens (start, play, game-over, overlays, …).
/// Each screen is responsible for updating its own state and drawing itself.
/// </summary>
/// <remarks>
/// Screens that need to trigger transitions or push overlays should inject
/// <see cref="IScreenCoordinator"/> in their constructor.
/// </remarks>
public abstract class GameScreen
{
    /// <summary>
    /// True while this screen has an overlay on top of it and is therefore not being updated.
    /// The screen is still drawn as the base layer while paused.
    /// </summary>
    public bool IsPaused { get; internal set; }

    // ── Virtual update and abstract draw ──────────────────────────────────

    /// <summary>
    /// Called each game tick to advance the game state. Not called while <see cref="IsPaused"/>.
    /// Override to implement per-frame logic. The default implementation does nothing.
    /// </summary>
    /// <param name="deltaTime">Seconds elapsed since the last update.</param>
    public virtual void Update(float deltaTime) { }

    /// <summary>
    /// Called each frame to render the current state to <paramref name="canvas"/>.
    /// The canvas is already transformed to game-space: the origin is at the top-left of the
    /// game area, one unit equals one game-space pixel, and the visible rectangle runs from
    /// (0, 0) to (<paramref name="width"/>, <paramref name="height"/>).
    /// </summary>
    /// <param name="canvas">The SkiaSharp canvas to draw on. Already in game-space coordinates.</param>
    /// <param name="width">Game-space width (equals <see cref="GameEngine.Game.GameDimensions"/>.Width).</param>
    /// <param name="height">Game-space height (equals <see cref="GameEngine.Game.GameDimensions"/>.Height).</param>
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
