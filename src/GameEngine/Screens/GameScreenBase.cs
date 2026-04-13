using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Abstract base class for all game screens (start, play, game-over, overlays, …).
/// Each screen is responsible for updating its own state and drawing itself.
/// </summary>
/// <remarks>
/// The owning <see cref="GameEngine.Game"/> injects itself via <see cref="SetGame"/> before
/// calling <see cref="OnActivated"/>. Use the protected <see cref="Game"/> property to trigger
/// screen transitions from within a screen implementation.
/// </remarks>
public abstract class GameScreenBase
{
    /// <summary>
    /// True while this screen has an overlay on top of it and is therefore not being updated.
    /// The screen is still drawn as the base layer while paused.
    /// </summary>
    public bool IsPaused { get; internal set; }

    /// <summary>
    /// The owning <see cref="GameEngine.Game"/>. Available after <see cref="OnActivated"/> is called.
    /// Use this to trigger transitions, push overlays, and pop overlays.
    /// </summary>
    protected Game? Game { get; private set; }

    /// <summary>Called by <see cref="GameEngine.Game"/> to inject itself before activation.</summary>
    internal void SetGame(Game game) => Game = game;

    // ── Abstract contract ─────────────────────────────────────────────────

    /// <summary>Called each game tick to update the game state. Not called while <see cref="IsPaused"/>.</summary>
    /// <param name="deltaTime">Seconds elapsed since the last update.</param>
    public abstract void Update(float deltaTime);

    /// <summary>
    /// Called each frame to render the current state to <paramref name="canvas"/>.
    /// The canvas is already transformed to game-space: the origin is at the top-left of the
    /// game area, one unit equals one game-space pixel, and the visible rectangle runs from
    /// (0, 0) to (<paramref name="width"/>, <paramref name="height"/>).
    /// </summary>
    /// <param name="canvas">The SkiaSharp canvas to draw on. Already in game-space coordinates.</param>
    /// <param name="width">Game-space width (equals <see cref="GameEngine.Game.GameDimensions"/>.width).</param>
    /// <param name="height">Game-space height (equals <see cref="GameEngine.Game.GameDimensions"/>.height).</param>
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
