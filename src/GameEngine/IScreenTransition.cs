using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Defines a visual transition effect between game states.
/// Implement this interface to create custom transitions such as fades, slides, or wipes.
/// </summary>
public interface IScreenTransition
{
    /// <summary>
    /// Renders the transition overlay onto the canvas.
    /// </summary>
    /// <param name="canvas">Canvas in game-space coordinates.</param>
    /// <param name="coverage">
    /// How much the transition covers the screen: 0 = not covering at all, 1 = fully covering.
    /// </param>
    /// <param name="dimensions">The game's logical (virtual) canvas dimensions.</param>
    void Draw(SKCanvas canvas, float coverage, (int width, int height) dimensions);
}
