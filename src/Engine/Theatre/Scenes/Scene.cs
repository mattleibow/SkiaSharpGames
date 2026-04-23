using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Abstract base class for all game scenes (start, play, game-over, layered scenes, …).
/// Each scene is responsible for updating its own state and drawing itself.
/// </summary>
/// <remarks>
/// Screens that need to trigger transitions or push scenes should inject
/// <see cref="IDirector"/> in their constructor.
/// </remarks>
public abstract class Scene : SceneNode
{
    /// <summary>
    /// Reference to the <see cref="Theatre.Stage"/> hosting this scene. Set by the
    /// <see cref="Director"/> when the scene is activated.
    /// </summary>
    public Stage? Stage { get; internal set; }

    /// <summary>
    /// Resolves the effective theme for this scene. Falls back through the parent chain
    /// and finally to <see cref="Stage.HudTheme"/>.
    /// </summary>
    public override HudTheme? ResolvedHudTheme =>
        HudTheme ?? Parent?.ResolvedHudTheme ?? Stage?.HudTheme;

    /// <summary>
    /// True while this scene has a layered scene on top of it and is therefore not being updated.
    /// The scene is still drawn as the base layer while paused.
    /// </summary>
    public bool IsPaused { get; internal set; }

    /// <summary>
    /// Optional visible pointer/cursor. When set, the engine automatically updates its
    /// position from pointer events. Draw it at the end of <see cref="Draw"/> to keep it on top.
    /// </summary>
    public HudPointer? Pointer { get; protected set; }

    // ── Virtual update and abstract draw ──────────────────────────────────

    /// <summary>
    /// Called each game tick to advance the game state. Not called while <see cref="IsPaused"/>.
    /// Override to implement per-frame logic. The default implementation does nothing.
    /// </summary>
    /// <param name="deltaTime">Seconds elapsed since the last update.</param>
    public new virtual void Update(float deltaTime) { }

    /// <summary>
    /// Called each frame to render the current state to <paramref name="canvas"/>.
    /// The canvas is already transformed to game-space: the origin is at the top-left of the
    /// game area, one unit equals one game-space pixel, and the visible rectangle runs from
    /// (0, 0) to (<paramref name="width"/>, <paramref name="height"/>).
    /// </summary>
    /// <param name="canvas">The SkiaSharp canvas to draw on. Already in game-space coordinates.</param>
    /// <param name="width">Stage-space width (equals <see cref="Theatre.Stage.StageSize"/>.Width).</param>
    /// <param name="height">Stage-space height (equals <see cref="Theatre.Stage.StageSize"/>.Height).</param>
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

    /// <summary>
    /// Called when a transition to this scene begins — i.e., the scene is about to become
    /// visible and is starting to appear. Input events are still forwarded to this scene during
    /// the transition so that games can produce interactive transition effects.
    /// Typically used to initialise game state so it is ready to be drawn during the animation.
    /// </summary>
    public virtual void OnActivating() { }

    /// <summary>
    /// Called once when this scene is fully visible and the transition (if any) has completed.
    /// Typically used to perform any final setup that depends on the scene being fully active,
    /// such as re-syncing dynamic elements to positions updated by input during the transition.
    /// </summary>
    public virtual void OnActivated() { }

    /// <summary>
    /// Called when a transition away from this scene begins — i.e., the scene is starting to
    /// disappear. Input events are still forwarded to this scene during the transition.
    /// </summary>
    public virtual void OnDeactivating() { }

    /// <summary>Called once when this scene is fully hidden and the transition has completed,
    /// or immediately when replaced without a transition.</summary>
    public virtual void OnDeactivated() { }

    /// <summary>Called when a layered scene is pushed on top of this scene.</summary>
    public virtual void OnPaused() { }

    /// <summary>Called when the last layered scene on top of this scene is removed.</summary>
    public virtual void OnResumed() { }
}
