using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// Base class for control appearances. Each appearance owns both visual
/// properties and the draw method for a specific control type.
/// </summary>
public abstract record UiAppearance<TEntity> where TEntity : Actor
{
    /// <summary>Draws the control onto the canvas.</summary>
    public abstract void Draw(SKCanvas canvas, TEntity entity);
}
