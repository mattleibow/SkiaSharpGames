namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Logical playfield bounds in game-space units.
/// </summary>
public readonly record struct GameBounds(float Left, float Top, float Right, float Bottom)
{
    public float Width => Right - Left;

    public float Height => Bottom - Top;

    public static GameBounds FromSize(float width, float height) => new(0f, 0f, width, height);
}
