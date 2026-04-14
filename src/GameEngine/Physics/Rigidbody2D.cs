namespace SkiaSharpGames.GameEngine;

/// <summary>
/// A velocity-driven movement component. Attach one to any <see cref="Entity"/> that should
/// move; static objects simply omit it.
/// </summary>
/// <remarks>
/// Call <see cref="Step"/> once per game tick to advance the owning entity's position
/// by <c>Velocity × deltaTime</c>.
/// </remarks>
public sealed class Rigidbody2D
{
    /// <summary>Horizontal velocity in game-space units per second.</summary>
    public float VelocityX { get; set; }

    /// <summary>Vertical velocity in game-space units per second.</summary>
    public float VelocityY { get; set; }

    /// <summary>Advances <paramref name="owner"/>'s position by velocity × <paramref name="deltaTime"/>.</summary>
    public void Step(Entity owner, float deltaTime)
    {
        owner.X += VelocityX * deltaTime;
        owner.Y += VelocityY * deltaTime;
    }
}
