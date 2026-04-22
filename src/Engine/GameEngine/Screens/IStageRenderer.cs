using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Handles the update and draw loop for the active screen stack.
/// </summary>
/// <remarks>
/// <see cref="Director"/> implements both this interface and
/// <see cref="IDirector"/>. Both are registered in the game's DI container so that
/// consumers can depend on the narrowest interface they need.
/// </remarks>
public interface IStageRenderer
{
    /// <summary>Advances the screen stack by <paramref name="deltaTime"/> seconds.</summary>
    void Update(float deltaTime);

    /// <summary>
    /// Draws the current screen (and any active transition or overlays) to
    /// <paramref name="canvas"/>. The canvas is assumed to already be in game-space
    /// coordinates (the fit-and-centre transform is applied by the caller).
    /// </summary>
    void Draw(SKCanvas canvas, int width, int height);
}
