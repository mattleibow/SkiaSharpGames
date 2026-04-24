namespace SkiaSharp.Theatre;

/// <summary>
/// Manages the active scene stack and transitions between scenes.
/// </summary>
/// <remarks>
/// Inject this interface into a <see cref="Scene"/> constructor to trigger transitions,
/// push scenes, and pop scenes.
/// </remarks>
public interface IDirector
{
    /// <summary>
    /// Replaces the current scene with a new <typeparamref name="TScene"/> instance,
    /// optionally playing a cross-scene transition. Clears any open layered scenes first.
    /// </summary>
    void TransitionTo<TScene>(ICurtain? transition = null)
        where TScene : Scene;

    /// <summary>
    /// Pushes a layered scene on top of the current scene.
    /// The current scene is paused (still drawn, not updated) while any layered scene is active.
    /// </summary>
    void PushScene<TScene>()
        where TScene : Scene;

    /// <summary>
    /// Removes the topmost layered scene and resumes the underlying scene.
    /// Does nothing if no layered scene is currently active.
    /// </summary>
    void PopScene();

    /// <summary>The scene that should receive input events.</summary>
    Scene ActiveInputScene { get; }
}