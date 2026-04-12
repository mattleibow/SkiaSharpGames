using SkiaSharp;

namespace SkiaSharpGames.GameEngine;

/// <summary>Shape of a <see cref="PhysicsBody"/>.</summary>
public enum PhysicsShape { Circle, Rect }

/// <summary>
/// A simple axis-aligned physics body. Supports two shapes:
/// <list type="bullet">
///   <item><b>Circle</b> — position (<see cref="X"/>, <see cref="Y"/>) is the centre.</item>
///   <item><b>Rect</b>   — position (<see cref="X"/>, <see cref="Y"/>) is the top-left corner.</item>
/// </list>
/// Bodies have a velocity and can be stepped forward in time via <see cref="Step"/>.
/// Static bodies (<see cref="IsStatic"/> = <see langword="true"/>) are not moved by <see cref="Step"/>.
/// </summary>
public sealed class PhysicsBody
{
    /// <summary>Shape of this body.</summary>
    public PhysicsShape Shape { get; }

    /// <summary>
    /// Horizontal position. Centre X for <see cref="PhysicsShape.Circle"/>;
    /// left edge for <see cref="PhysicsShape.Rect"/>.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Vertical position. Centre Y for <see cref="PhysicsShape.Circle"/>;
    /// top edge for <see cref="PhysicsShape.Rect"/>.
    /// </summary>
    public float Y { get; set; }

    /// <summary>Horizontal velocity in game-space units per second.</summary>
    public float VelocityX { get; set; }

    /// <summary>Vertical velocity in game-space units per second.</summary>
    public float VelocityY { get; set; }

    /// <summary>Radius. Only meaningful when <see cref="Shape"/> is <see cref="PhysicsShape.Circle"/>.</summary>
    public float Radius { get; set; }

    /// <summary>Width. Only meaningful when <see cref="Shape"/> is <see cref="PhysicsShape.Rect"/>.</summary>
    public float Width { get; set; }

    /// <summary>Height. Only meaningful when <see cref="Shape"/> is <see cref="PhysicsShape.Rect"/>.</summary>
    public float Height { get; set; }

    /// <summary>When <see langword="true"/> <see cref="Step"/> does not modify position.</summary>
    public bool IsStatic { get; set; }

    /// <summary>Creates a new body with the given shape.</summary>
    public PhysicsBody(PhysicsShape shape) => Shape = shape;

    /// <summary>Advances position by velocity × <paramref name="deltaTime"/>. No-op when <see cref="IsStatic"/>.</summary>
    public void Step(float deltaTime)
    {
        if (IsStatic) return;
        X += VelocityX * deltaTime;
        Y += VelocityY * deltaTime;
    }

    /// <summary>The axis-aligned bounding box of this body.</summary>
    public SKRect BoundingBox => Shape == PhysicsShape.Circle
        ? new SKRect(X - Radius, Y - Radius, X + Radius, Y + Radius)
        : new SKRect(X, Y, X + Width, Y + Height);

    /// <summary>Returns <see langword="true"/> when this body's area overlaps <paramref name="other"/>.</summary>
    public bool Overlaps(PhysicsBody other) => (Shape, other.Shape) switch
    {
        (PhysicsShape.Circle, PhysicsShape.Circle) => CircleCircle(this, other),
        (PhysicsShape.Circle, PhysicsShape.Rect)   => CircleRect(this, other),
        (PhysicsShape.Rect,   PhysicsShape.Circle) => CircleRect(other, this),
        (PhysicsShape.Rect,   PhysicsShape.Rect)   => RectRect(this, other),
        _                                           => false
    };

    /// <summary>
    /// Reflects this body's velocity off <paramref name="other"/> using the smallest-overlap axis.
    /// Only valid when this body is a <see cref="PhysicsShape.Circle"/> and <paramref name="other"/>
    /// is a <see cref="PhysicsShape.Rect"/>. Does <b>not</b> check for overlap first; call
    /// <see cref="ReflectOff"/> if you need the overlap check included.
    /// </summary>
    public void Reflect(PhysicsBody rect)
    {
        if (Shape != PhysicsShape.Circle || rect.Shape != PhysicsShape.Rect) return;

        float overlapLeft   = (X + Radius) - rect.X;
        float overlapRight  = (rect.X + rect.Width)  - (X - Radius);
        float overlapTop    = (Y + Radius) - rect.Y;
        float overlapBottom = (rect.Y + rect.Height) - (Y - Radius);
        float minH = MathF.Min(overlapLeft, overlapRight);
        float minV = MathF.Min(overlapTop,  overlapBottom);

        if (minV <= minH) VelocityY = -VelocityY;
        else              VelocityX = -VelocityX;
    }

    /// <summary>
    /// If this body overlaps <paramref name="other"/>, reflects velocity and returns
    /// <see langword="true"/>. Combines <see cref="Overlaps"/> + <see cref="Reflect"/> for
    /// convenience.
    /// </summary>
    public bool ReflectOff(PhysicsBody other)
    {
        if (!Overlaps(other)) return false;

        if (Shape == PhysicsShape.Circle && other.Shape == PhysicsShape.Circle)
        {
            float dx = X - other.X, dy = Y - other.Y;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            if (len > 0f)
            {
                float nx = dx / len, ny = dy / len;
                float dot = VelocityX * nx + VelocityY * ny;
                VelocityX -= 2f * dot * nx;
                VelocityY -= 2f * dot * ny;
            }
            return true;
        }

        Reflect(other.Shape == PhysicsShape.Rect ? other : this);
        return true;
    }

    // ── Internal helpers ──────────────────────────────────────────────────

    private static bool CircleCircle(PhysicsBody a, PhysicsBody b)
    {
        float dx = a.X - b.X, dy = a.Y - b.Y, r = a.Radius + b.Radius;
        return dx * dx + dy * dy < r * r;
    }

    private static bool CircleRect(PhysicsBody c, PhysicsBody r)
    {
        float nearX = Math.Clamp(c.X, r.X, r.X + r.Width);
        float nearY = Math.Clamp(c.Y, r.Y, r.Y + r.Height);
        float dx = c.X - nearX, dy = c.Y - nearY;
        return dx * dx + dy * dy < c.Radius * c.Radius;
    }

    private static bool RectRect(PhysicsBody a, PhysicsBody b)
        => a.BoundingBox.IntersectsWith(b.BoundingBox);
}
