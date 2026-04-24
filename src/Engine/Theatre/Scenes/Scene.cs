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
    /// Creates a new scene with an auto-created <see cref="Pointer"/>.
    /// The pointer's <see cref="SceneNode.Parent"/> is set to this scene for theme resolution
    /// but it is not added to <see cref="SceneNode.Children"/> — the engine draws it
    /// on top of all scene content via the <see cref="Director"/>.
    /// </summary>
    protected Scene()
    {
        var pointer = new HudPointer();
        pointer.Parent = this;
        Pointer = pointer;
    }

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
    /// Visible pointer/cursor. Auto-created for every scene. The engine automatically
    /// updates its position from pointer events and draws it on top of all scene content.
    /// Use <see cref="ShowPointer"/> to hide the pointer for scenes that don't need it.
    /// </summary>
    public HudPointer Pointer { get; protected set; }

    /// <summary>
    /// Controls whether the pointer is drawn for this scene.
    /// <c>null</c> (default) inherits from <see cref="Stage.ShowPointer"/>.
    /// Set to <c>false</c> on play screens where the cursor is not needed
    /// (e.g., paddle-follow or keyboard-only games).
    /// </summary>
    public bool? ShowPointer { get; set; }

    /// <summary>
    /// Resolves the effective pointer visibility for this scene:
    /// <see cref="ShowPointer"/> if set, otherwise <see cref="Stage.ShowPointer"/>, else true.
    /// </summary>
    internal bool EffectiveShowPointer => ShowPointer ?? Stage?.ShowPointer ?? true;

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
