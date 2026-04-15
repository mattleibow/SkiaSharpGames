using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Base class for all drawable game sprites.
/// A sprite draws at the canvas origin (0, 0) — the entity's <see cref="Entity.Draw"/>
/// method translates the canvas to the entity's position before calling
/// <see cref="Draw(SKCanvas)"/>.
/// </summary>
public abstract class Sprite
{
    /// <summary>Opacity from 0 (invisible) to 1 (fully opaque).</summary>
    public float Alpha { get; set; } = 1f;

    /// <summary>When false the sprite is skipped during rendering.</summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Renders the sprite onto <paramref name="canvas"/> centred at the canvas origin.
    /// The canvas has already been translated to the entity's position.
    /// </summary>
    public abstract void Draw(SKCanvas canvas);

    /// <summary>
    /// Advances any animated properties by <paramref name="deltaTime"/> seconds.
    /// Override in subclasses to drive per-sprite animations.
    /// </summary>
    public virtual void Update(float deltaTime) { }
}
