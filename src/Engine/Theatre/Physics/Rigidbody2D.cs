namespace SkiaSharp.Theatre;

/// <summary>
/// A velocity-driven movement component. Attach one to any <see cref="Actor"/> that should
/// move; static objects simply omit it.
/// </summary>
/// <remarks>
/// Call <see cref="Step"/> once per game tick to advance the owning actor's position
/// by <c>Velocity × deltaTime</c>.
/// </remarks>
public sealed class Rigidbody2D
{
    /// <summary>Horizontal velocity in game-space units per second.</summary>
    public float VelocityX { get; set; }

    /// <summary>Vertical velocity in game-space units per second.</summary>
    public float VelocityY { get; set; }

    /// <summary>Current speed magnitude in game-space units per second.</summary>
    public float Speed => MathF.Sqrt(VelocityX * VelocityX + VelocityY * VelocityY);

    /// <summary>Sets the velocity directly.</summary>
    public void SetVelocity(float velocityX, float velocityY)
    {
        VelocityX = velocityX;
        VelocityY = velocityY;
    }

    /// <summary>Adds to the current velocity.</summary>
    public void AddVelocity(float deltaX, float deltaY)
    {
        VelocityX += deltaX;
        VelocityY += deltaY;
    }

    /// <summary>Stops the body immediately.</summary>
    public void Stop()
    {
        VelocityX = 0f;
        VelocityY = 0f;
    }

    /// <summary>
    /// Advances <paramref name="owner"/>'s position by velocity × <paramref name="deltaTime"/>.
    /// </summary>
    public void Step(Actor owner, float deltaTime)
    {
        owner.X += VelocityX * deltaTime;
        owner.Y += VelocityY * deltaTime;
    }

    /// <summary>Reflects horizontal velocity.</summary>
    public void BounceX(float restitution = 1f) =>
        VelocityX = -VelocityX * MathF.Max(0f, restitution);

    /// <summary>Reflects vertical velocity.</summary>
    public void BounceY(float restitution = 1f) =>
        VelocityY = -VelocityY * MathF.Max(0f, restitution);

    /// <summary>Reflects velocity using a collision normal.</summary>
    public void Bounce(float normalX, float normalY, float restitution = 1f)
    {
        float lengthSquared = normalX * normalX + normalY * normalY;
        if (lengthSquared <= 0f)
            return;

        float invLength = 1f / MathF.Sqrt(lengthSquared);
        float nx = normalX * invLength;
        float ny = normalY * invLength;
        float dot = VelocityX * nx + VelocityY * ny;

        if (dot >= 0f)
            return;

        float bounce = 1f + MathF.Max(0f, restitution);
        VelocityX -= bounce * dot * nx;
        VelocityY -= bounce * dot * ny;
    }

    /// <summary>Reflects velocity using the normal from a collision result.</summary>
    public void Bounce(CollisionHit hit, float restitution = 1f) =>
        Bounce(hit.NormalX, hit.NormalY, restitution);
}