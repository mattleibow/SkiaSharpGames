namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Abstract base for all game objects that exist in the game world.
/// An entity is the single source of truth for position; physics and rendering
/// components read from it rather than storing their own copy.
/// </summary>
public abstract class Entity
{
    /// <summary>Horizontal centre position in game-space units.</summary>
    public float X { get; set; }

    /// <summary>Vertical centre position in game-space units.</summary>
    public float Y { get; set; }

    /// <summary>When <see langword="false"/> the entity is skipped during update and rendering.</summary>
    public bool Active { get; set; } = true;
}
