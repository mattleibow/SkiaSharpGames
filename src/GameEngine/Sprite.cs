using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Base class for all drawable game sprites.
/// A sprite encapsulates its own visual representation and draws itself onto the canvas.
/// </summary>
public abstract class Sprite
{
    /// <summary>Horizontal position (left edge in game-space units).</summary>
    public float X { get; set; }

    /// <summary>Vertical position (top edge in game-space units).</summary>
    public float Y { get; set; }

    /// <summary>Opacity from 0 (invisible) to 1 (fully opaque).</summary>
    public float Alpha { get; set; } = 1f;

    /// <summary>When false the sprite is skipped during rendering.</summary>
    public bool Visible { get; set; } = true;

    /// <summary>Renders the sprite onto <paramref name="canvas"/>.</summary>
    public abstract void Draw(SKCanvas canvas);
}
