using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>
/// Abstract base class for all physics bodies.
/// A body has a position, velocity, and an axis-aligned bounding box.
/// Concrete subclasses — <see cref="CircleBody"/> and <see cref="RectBody"/> — define the shape.
/// </summary>
/// <remarks>
/// Bodies can be stepped forward in time, queried for overlap, and reflected off one another
/// using the <see cref="CollisionResolver"/> helpers (also available as instance methods for
/// convenience).
/// Static bodies (<see cref="IsStatic"/> = <see langword="true"/>) are not moved by <see cref="Step"/>.
/// </remarks>
public abstract class PhysicsBody
{
    /// <summary>
    /// Horizontal position in game-space units.
    /// For <see cref="CircleBody"/> this is the centre X; for <see cref="RectBody"/> it is the left edge.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Vertical position in game-space units.
    /// For <see cref="CircleBody"/> this is the centre Y; for <see cref="RectBody"/> it is the top edge.
    /// </summary>
    public float Y { get; set; }

    /// <summary>Horizontal velocity in game-space units per second.</summary>
    public float VelocityX { get; set; }

    /// <summary>Vertical velocity in game-space units per second.</summary>
    public float VelocityY { get; set; }

    /// <summary>When <see langword="true"/>, <see cref="Step"/> does not modify position.</summary>
    public bool IsStatic { get; set; }

    /// <summary>Advances position by velocity × <paramref name="deltaTime"/>. No-op when <see cref="IsStatic"/>.</summary>
    public virtual void Step(float deltaTime)
    {
        if (IsStatic) return;
        X += VelocityX * deltaTime;
        Y += VelocityY * deltaTime;
    }

    /// <summary>The axis-aligned bounding box of this body in game-space units.</summary>
    public abstract SKRect BoundingBox { get; }

    /// <summary>Returns <see langword="true"/> when this body's area overlaps <paramref name="other"/>.</summary>
    public bool Overlaps(PhysicsBody other) => CollisionResolver.Overlaps(this, other);

    /// <summary>
    /// Reflects this body's velocity off <paramref name="other"/> using the smallest-overlap axis.
    /// Only has an effect when this body is a <see cref="CircleBody"/> and <paramref name="other"/>
    /// is a <see cref="RectBody"/>. Does <b>not</b> check for overlap first — call
    /// <see cref="ReflectOff"/> when you need the overlap check included.
    /// </summary>
    public void Reflect(PhysicsBody other) => CollisionResolver.Reflect(this, other);

    /// <summary>
    /// If this body overlaps <paramref name="other"/>, reflects velocity and returns
    /// <see langword="true"/>. Combines <see cref="Overlaps"/> + reflection for convenience.
    /// </summary>
    public bool ReflectOff(PhysicsBody other) => CollisionResolver.ReflectOff(this, other);
}
