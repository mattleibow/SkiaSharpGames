using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Abstract base class for game screens. Implement this to create
/// different game screens such as start, play, and game over screens.
/// </summary>
public abstract class GameScreenBase
{
    /// <summary>
    /// Gets the logical (virtual) dimensions of the game canvas in game-space units.
    /// The default is 800 × 600. Override in subclasses to change.
    /// </summary>
    public virtual (int width, int height) GameDimensions => (800, 600);

    /// <summary>
    /// Called each game tick to update the game state.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last update, in seconds.</param>
    public abstract void Update(float deltaTime);

    /// <summary>
    /// Called each frame to render the current state.
    /// </summary>
    /// <param name="canvas">The SkiaSharp canvas to draw on.</param>
    /// <param name="width">The width of the canvas in pixels.</param>
    /// <param name="height">The height of the canvas in pixels.</param>
    public abstract void Draw(SKCanvas canvas, int width, int height);

    /// <summary>
    /// Called when the pointer/mouse moves over the canvas.
    /// </summary>
    /// <param name="x">Horizontal position in game-space units.</param>
    /// <param name="y">Vertical position in game-space units.</param>
    public virtual void OnPointerMove(float x, float y) { }

    /// <summary>
    /// Called when the user clicks or taps the canvas.
    /// </summary>
    /// <param name="x">Horizontal position in game-space units.</param>
    /// <param name="y">Vertical position in game-space units.</param>
    public virtual void OnPointerDown(float x, float y) { }

    /// <summary>
    /// Called when a key is pressed while the game canvas has focus.
    /// </summary>
    /// <param name="key">The key value string (e.g. "ArrowLeft", " ", "z").</param>
    public virtual void OnKeyDown(string key) { }

    /// <summary>
    /// Called when a key is released while the game canvas has focus.
    /// </summary>
    /// <param name="key">The key value string (e.g. "ArrowLeft", " ", "z").</param>
    public virtual void OnKeyUp(string key) { }

    // ── Transition system ─────────────────────────────────────────────────

    private enum TransitionPhase { None, Out, In }

    private TransitionPhase _transitionPhase = TransitionPhase.None;
    private float _transitionProgress;
    private float _transitionHalfDuration;
    private IScreenTransition? _activeTransition;
    private Action? _transitionMidpoint;

    /// <summary>True while a transition animation is running.</summary>
    public bool IsTransitioning => _transitionPhase != TransitionPhase.None;

    /// <summary>
    /// Begins an animated transition. The <paramref name="midpointAction"/> fires once the
    /// transition overlay fully covers the screen; use it to change game state. The overlay
    /// then retreats to reveal the new state.
    /// </summary>
    /// <param name="transition">The transition style to use (e.g. <see cref="FadeTransition"/>).</param>
    /// <param name="halfDuration">Duration (seconds) for each half of the transition.</param>
    /// <param name="midpointAction">State-change callback invoked at peak coverage.</param>
    protected void BeginTransition(IScreenTransition transition, float halfDuration, Action midpointAction)
    {
        _activeTransition = transition;
        _transitionHalfDuration = halfDuration;
        _transitionProgress = 0f;
        _transitionPhase = TransitionPhase.Out;
        _transitionMidpoint = midpointAction;
    }

    /// <summary>
    /// Advances the active transition. Call this at the top of your <see cref="Update"/> implementation.
    /// </summary>
    protected void UpdateTransition(float deltaTime)
    {
        if (_transitionPhase == TransitionPhase.None) return;

        _transitionProgress += deltaTime / _transitionHalfDuration;

        if (_transitionProgress >= 1f)
        {
            if (_transitionPhase == TransitionPhase.Out)
            {
                _transitionMidpoint?.Invoke();
                _transitionMidpoint = null;
                _transitionProgress = 0f;
                _transitionPhase = TransitionPhase.In;
            }
            else
            {
                _transitionProgress = 1f;
                _transitionPhase = TransitionPhase.None;
                _activeTransition = null;
            }
        }
    }

    /// <summary>
    /// Renders the transition overlay. Call this at the end of your <see cref="Draw"/> implementation,
    /// inside any active canvas transforms (i.e. in game-space coordinates).
    /// </summary>
    protected void DrawTransitionOverlay(SKCanvas canvas)
    {
        if (_transitionPhase == TransitionPhase.None || _activeTransition == null) return;

        float coverage = _transitionPhase == TransitionPhase.Out
            ? _transitionProgress
            : 1f - _transitionProgress;

        _activeTransition.Draw(canvas, coverage, GameDimensions);
    }
}

