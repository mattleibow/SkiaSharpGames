namespace SkiaSharp.Theatre;

/// <summary>
/// Controls how the pointer/cursor is displayed for a scene.
/// </summary>
public enum PointerPolicy
{
    /// <summary>
    /// Pointer is always visible when the mouse is over the canvas.
    /// Use for menus, start/game-over screens, and games where the cursor
    /// is needed to aim or interact (e.g., CastleAttack, UIGallery).
    /// </summary>
    AlwaysVisible,

    /// <summary>
    /// Pointer is never drawn. Use for gameplay scenes where the mouse
    /// directly controls a game object (paddle-follow) or the game is
    /// keyboard-only. The pointer position is still tracked for hit-testing.
    /// </summary>
    AlwaysHidden,

    /// <summary>
    /// Pointer is visible on movement, then fades out after
    /// <see cref="Scene.PointerIdleTimeout"/> seconds of no movement.
    /// Use for games with on-screen touch buttons where the cursor helps
    /// find buttons but is distracting during keyboard play.
    /// </summary>
    HideWhenIdle,
}
