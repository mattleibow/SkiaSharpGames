using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Handles the update and draw loop for the active scene stack.
/// </summary>
/// <remarks>
/// <see cref="Director"/> implements both this interface and
/// <see cref="IDirector"/>. Both are registered in the game's DI container so that
/// consumers can depend on the narrowest interface they need.
/// </remarks>
public interface IRenderer
{
    /// <summary>Advances the scene stack by <paramref name="deltaTime"/> seconds.</summary>
    void Update(float deltaTime);

    /// <summary>
    /// Draws the current scene (and any active transition or layered scenes) to
    /// <paramref name="canvas"/>. The canvas is assumed to already be in game-space
    /// coordinates (the fit-and-centre transform is applied by the caller).
    /// </summary>
    void Draw(SKCanvas canvas);
}
