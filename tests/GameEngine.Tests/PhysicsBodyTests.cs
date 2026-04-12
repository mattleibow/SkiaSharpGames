using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

/// <summary>
/// Tests for the physics body hierarchy and <see cref="CollisionResolver"/>,
/// targeting 100% branch coverage on all physics classes.
/// </summary>
public class PhysicsBodyTests
{
    // ── CircleBody construction / BoundingBox ─────────────────────────────

    [Fact]
    public void CircleBody_BoundingBox_CentredOnXY()
    {
        var b = new CircleBody { X = 100f, Y = 200f, Radius = 10f };
        var bb = b.BoundingBox;
        Assert.Equal(90f,  bb.Left,   precision: 4);
        Assert.Equal(190f, bb.Top,    precision: 4);
        Assert.Equal(110f, bb.Right,  precision: 4);
        Assert.Equal(210f, bb.Bottom, precision: 4);
    }

    // ── RectBody construction / BoundingBox ───────────────────────────────

    [Fact]
    public void RectBody_BoundingBox_TopLeftAtXY()
    {
        var b = new RectBody { X = 10f, Y = 20f, Width = 80f, Height = 30f };
        var bb = b.BoundingBox;
        Assert.Equal(10f, bb.Left,   precision: 4);
        Assert.Equal(20f, bb.Top,    precision: 4);
        Assert.Equal(90f, bb.Right,  precision: 4);
        Assert.Equal(50f, bb.Bottom, precision: 4);
    }

    // ── Step ──────────────────────────────────────────────────────────────

    [Fact]
    public void Step_NonStatic_MovesBody()
    {
        var b = new CircleBody { X = 10f, Y = 20f, VelocityX = 100f, VelocityY = -50f };
        b.Step(0.5f);
        Assert.Equal(60f, b.X, precision: 4);
        Assert.Equal(-5f, b.Y, precision: 4);
    }

    [Fact]
    public void Step_Static_DoesNotMove()
    {
        var b = new RectBody { X = 5f, Y = 5f, VelocityX = 100f, VelocityY = 100f, IsStatic = true };
        b.Step(1f);
        Assert.Equal(5f, b.X);
        Assert.Equal(5f, b.Y);
    }

    // ── CollisionResolver.Overlaps — Circle vs Circle ─────────────────────

    [Fact]
    public void Overlaps_CircleCircle_WhenOverlapping_ReturnsTrue()
    {
        var a = new CircleBody { X = 0f, Y = 0f, Radius = 10f };
        var b = new CircleBody { X = 5f, Y = 0f, Radius = 10f };
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_CircleCircle_WhenNotOverlapping_ReturnsFalse()
    {
        var a = new CircleBody { X = 0f,   Y = 0f, Radius = 5f };
        var b = new CircleBody { X = 100f, Y = 0f, Radius = 5f };
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_CircleCircle_Symmetric()
    {
        var a = new CircleBody { X = 0f, Y = 0f, Radius = 10f };
        var b = new CircleBody { X = 5f, Y = 0f, Radius = 10f };
        Assert.Equal(a.Overlaps(b), b.Overlaps(a));
    }

    // ── CollisionResolver.Overlaps — Circle vs Rect ───────────────────────

    [Fact]
    public void Overlaps_CircleRect_WhenCircleInsideRect_ReturnsTrue()
    {
        var circle = new CircleBody { X = 50f, Y = 50f, Radius = 5f };
        var rect   = new RectBody   { X = 0f,  Y = 0f,  Width = 100f, Height = 100f };
        Assert.True(circle.Overlaps(rect));
    }

    [Fact]
    public void Overlaps_CircleRect_WhenCircleTouchesEdge_ReturnsTrue()
    {
        var circle = new CircleBody { X = 109f, Y = 50f, Radius = 10f };
        var rect   = new RectBody   { X = 0f,   Y = 0f,  Width = 100f, Height = 100f };
        Assert.True(circle.Overlaps(rect));
    }

    [Fact]
    public void Overlaps_CircleRect_WhenCircleFarAway_ReturnsFalse()
    {
        var circle = new CircleBody { X = 500f, Y = 500f, Radius = 5f };
        var rect   = new RectBody   { X = 0f,   Y = 0f,   Width = 100f, Height = 100f };
        Assert.False(circle.Overlaps(rect));
    }

    // ── CollisionResolver.Overlaps — Rect vs Circle ───────────────────────

    [Fact]
    public void Overlaps_RectCircle_AgreesWithCircleRect()
    {
        var circle = new CircleBody { X = 50f, Y = 50f, Radius = 5f };
        var rect   = new RectBody   { X = 0f,  Y = 0f,  Width = 100f, Height = 100f };
        Assert.Equal(circle.Overlaps(rect), rect.Overlaps(circle));
    }

    [Fact]
    public void Overlaps_RectCircle_WhenNotOverlapping_ReturnsFalse()
    {
        var circle = new CircleBody { X = 500f, Y = 500f, Radius = 5f };
        var rect   = new RectBody   { X = 0f,   Y = 0f,   Width = 100f, Height = 100f };
        Assert.False(rect.Overlaps(circle));
    }

    // ── CollisionResolver.Overlaps — Rect vs Rect ─────────────────────────

    [Fact]
    public void Overlaps_RectRect_WhenOverlapping_ReturnsTrue()
    {
        var a = new RectBody { X = 0f,  Y = 0f,  Width = 50f, Height = 50f };
        var b = new RectBody { X = 25f, Y = 25f, Width = 50f, Height = 50f };
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_RectRect_WhenNotOverlapping_ReturnsFalse()
    {
        var a = new RectBody { X = 0f,   Y = 0f,   Width = 50f, Height = 50f };
        var b = new RectBody { X = 200f, Y = 200f, Width = 50f, Height = 50f };
        Assert.False(a.Overlaps(b));
    }

    // ── CollisionResolver.Reflect — circle hits top of rect (flip Y) ──────

    [Fact]
    public void Reflect_CircleHitsTopOfRect_FlipsVelocityY()
    {
        // Ball barely overlapping top edge → small vertical overlap, large horizontal overlap
        // minV < minH → flip Y
        var ball = new CircleBody
        {
            X = 50f, Y = 19f, Radius = 8f,  // bottom at 27, brick top at 20 → overlapTop=7
            VelocityX = 0f, VelocityY = 200f
        };
        var brick = new RectBody { X = 0f, Y = 20f, Width = 100f, Height = 22f };

        ball.Reflect(brick);

        Assert.True(ball.VelocityY < 0f, "VelocityY should be flipped to negative");
        Assert.Equal(0f, ball.VelocityX, precision: 4);
    }

    // ── CollisionResolver.Reflect — circle hits side of rect (flip X) ─────

    [Fact]
    public void Reflect_CircleHitsSideOfRect_FlipsVelocityX()
    {
        // Ball barely overlapping left side → small horizontal overlap, large vertical overlap
        // minH < minV → flip X
        var ball = new CircleBody
        {
            // X=3, Radius=8: right=11, rect left=0 → overlapLeft=11, overlapRight=105, minH=11
            // Y=50, rect top=0,h=200: overlapTop=58, overlapBottom=158, minV=58
            // minH(11) < minV(58) → flip X
            X = 3f, Y = 50f, Radius = 8f,
            VelocityX = 200f, VelocityY = 0f
        };
        var brick = new RectBody { X = 0f, Y = 0f, Width = 100f, Height = 200f };

        ball.Reflect(brick);

        Assert.True(ball.VelocityX < 0f, "VelocityX should be flipped to negative");
        Assert.Equal(0f, ball.VelocityY, precision: 4);
    }

    // ── CollisionResolver.Reflect — non-circle caller is a no-op ──────────

    [Fact]
    public void Reflect_WhenCallerIsRect_IsNoOp()
    {
        // Reflect only acts when 'a' is CircleBody — all other cases are no-ops
        var rect1 = new RectBody { VelocityX = 1f, VelocityY = 1f };
        var rect2 = new RectBody { X = 0f, Y = 0f, Width = 100f, Height = 100f };
        rect1.Reflect(rect2);
        Assert.Equal(1f, rect1.VelocityX);
        Assert.Equal(1f, rect1.VelocityY);
    }

    // ── CollisionResolver.ReflectOff — no overlap ─────────────────────────

    [Fact]
    public void ReflectOff_WhenNoOverlap_ReturnsFalse_VelocityUnchanged()
    {
        var ball  = new CircleBody { X = 500f, Y = 500f, Radius = 5f, VelocityX = 1f, VelocityY = 1f };
        var brick = new RectBody   { X = 0f,   Y = 0f,   Width = 10f, Height = 10f };
        bool result = ball.ReflectOff(brick);
        Assert.False(result);
        Assert.Equal(1f, ball.VelocityX);
        Assert.Equal(1f, ball.VelocityY);
    }

    // ── CollisionResolver.ReflectOff — Circle vs Rect ─────────────────────

    [Fact]
    public void ReflectOff_CircleVsRect_WhenOverlapping_ReturnsTrue_AndReflects()
    {
        var ball  = new CircleBody { X = 50f, Y = 19f, Radius = 8f, VelocityX = 0f, VelocityY = 200f };
        var brick = new RectBody   { X = 0f,  Y = 20f, Width = 100f, Height = 22f };

        bool result = ball.ReflectOff(brick);

        Assert.True(result);
        Assert.True(ball.VelocityY < 0f);
    }

    // ── CollisionResolver.ReflectOff — Rect vs Circle ─────────────────────

    [Fact]
    public void ReflectOff_RectVsCircle_WhenOverlapping_ReturnsTrue()
    {
        // Goes into the else-branch of ReflectOff → calls Reflect(rect, circle)
        // which is a no-op (circle is not the caller) → returns true, velocity unchanged
        var rect   = new RectBody   { X = 0f,  Y = 0f,  Width = 100f, Height = 100f, VelocityX = 1f, VelocityY = 1f };
        var circle = new CircleBody { X = 50f, Y = 50f, Radius = 5f };

        bool result = rect.ReflectOff(circle);

        Assert.True(result);
    }

    // ── CollisionResolver.ReflectOff — Rect vs Rect ───────────────────────

    [Fact]
    public void ReflectOff_RectVsRect_WhenOverlapping_ReturnsTrue()
    {
        var a = new RectBody { X = 0f, Y = 0f, Width = 50f, Height = 50f, VelocityX = 1f };
        var b = new RectBody { X = 25f, Y = 25f, Width = 50f, Height = 50f };
        bool result = a.ReflectOff(b);
        Assert.True(result);
    }

    // ── CollisionResolver.ReflectOff — Circle vs Circle ───────────────────

    [Fact]
    public void ReflectOff_CircleCircle_WhenOverlapping_ReflectsVelocity()
    {
        var a = new CircleBody { X = 0f, Y = 0f, Radius = 10f, VelocityX = 100f, VelocityY = 0f };
        var b = new CircleBody { X = 5f, Y = 0f, Radius = 10f };

        bool result = a.ReflectOff(b);

        Assert.True(result);
        // Ball was moving right (+X), hit ball to the right → velocity should be deflected
        Assert.True(a.VelocityX < 100f, "VelocityX should have been reduced by reflection");
    }

    [Fact]
    public void ReflectOff_CircleCircle_SamePosition_SkipsReflect_StillReturnsTrue()
    {
        // len == 0 branch: both circles at same position — reflection skipped, returns true
        var a = new CircleBody { X = 0f, Y = 0f, Radius = 10f, VelocityX = 50f, VelocityY = 50f };
        var b = new CircleBody { X = 0f, Y = 0f, Radius = 10f };

        bool result = a.ReflectOff(b);

        Assert.True(result);
        Assert.Equal(50f, a.VelocityX, precision: 4);
        Assert.Equal(50f, a.VelocityY, precision: 4);
    }
}
