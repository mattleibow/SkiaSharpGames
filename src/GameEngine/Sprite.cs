using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Base class for all drawable game sprites.
/// A sprite encapsulates visual appearance only — position is not stored here.
/// Callers pass the position at draw time so that the entity remains the single
/// source of truth for where things are.
/// </summary>
public abstract class Sprite
{
    /// <summary>Opacity from 0 (invisible) to 1 (fully opaque).</summary>
    public float Alpha { get; set; } = 1f;

    /// <summary>When false the sprite is skipped during rendering.</summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Renders the sprite onto <paramref name="canvas"/> centred on
    /// (<paramref name="x"/>, <paramref name="y"/>) in game-space units.
    /// </summary>
    public abstract void Draw(SKCanvas canvas, float x, float y);

    /// <summary>
    /// Advances any animated properties by <paramref name="deltaTime"/> seconds.
    /// Override in subclasses to drive per-sprite animations.
    /// </summary>
    public virtual void Update(float deltaTime) { }
}
