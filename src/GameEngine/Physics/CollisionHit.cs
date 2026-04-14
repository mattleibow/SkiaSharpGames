namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Result of a 2D overlap test. The normal points in the direction the first object should
/// move or bounce to separate from the second object.
/// </summary>
public readonly record struct CollisionHit(float NormalX, float NormalY, float Penetration)
{
    public bool IsHorizontal => MathF.Abs(NormalX) > MathF.Abs(NormalY);

    public bool IsVertical => MathF.Abs(NormalY) >= MathF.Abs(NormalX);
}
