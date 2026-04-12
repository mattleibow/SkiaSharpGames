using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Abstract base class for game screens. Implement this to create
/// different game screens such as start, play, and game over screens.
/// </summary>
public abstract class GameScreenBase
{
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
    /// <param name="x">Horizontal position in canvas pixels.</param>
    /// <param name="y">Vertical position in canvas pixels.</param>
    public virtual void OnPointerMove(float x, float y) { }

    /// <summary>
    /// Called when the user clicks or taps the canvas.
    /// </summary>
    /// <param name="x">Horizontal position in canvas pixels.</param>
    /// <param name="y">Vertical position in canvas pixels.</param>
    public virtual void OnPointerDown(float x, float y) { }
}
