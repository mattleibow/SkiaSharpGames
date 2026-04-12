using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

/// <summary>
/// Tests for <see cref="PhysicsBody"/> targeting 100% branch coverage.
/// </summary>
public class PhysicsBodyTests
{
    // ── Construction ──────────────────────────────────────────────────────

    [Fact]
    public void Constructor_Circle_SetsShape()
    {
        var b = new PhysicsBody(PhysicsShape.Circle);
        Assert.Equal(PhysicsShape.Circle, b.Shape);
    }

    [Fact]
    public void Constructor_Rect_SetsShape()
    {
        var b = new PhysicsBody(PhysicsShape.Rect);
        Assert.Equal(PhysicsShape.Rect, b.Shape);
    }

    // ── Step ──────────────────────────────────────────────────────────────

    [Fact]
    public void Step_NonStatic_MovesBody()
    {
        var b = new PhysicsBody(PhysicsShape.Circle)
            { X = 10f, Y = 20f, VelocityX = 100f, VelocityY = -50f };
        b.Step(0.5f);
        Assert.Equal(60f, b.X, precision: 4);
        Assert.Equal(-5f, b.Y, precision: 4);
    }

    [Fact]
    public void Step_Static_DoesNotMove()
    {
        var b = new PhysicsBody(PhysicsShape.Rect)
            { X = 5f, Y = 5f, VelocityX = 100f, VelocityY = 100f, IsStatic = true };
        b.Step(1f);
        Assert.Equal(5f, b.X);
        Assert.Equal(5f, b.Y);
    }

    // ── BoundingBox ───────────────────────────────────────────────────────

    [Fact]
    public void BoundingBox_Circle_CentredOnXY()
    {
        var b = new PhysicsBody(PhysicsShape.Circle) { X = 100f, Y = 200f, Radius = 10f };
        var bb = b.BoundingBox;
        Assert.Equal(90f, bb.Left,   precision: 4);
        Assert.Equal(190f, bb.Top,   precision: 4);
        Assert.Equal(110f, bb.Right, precision: 4);
        Assert.Equal(210f, bb.Bottom,precision: 4);
    }

    [Fact]
    public void BoundingBox_Rect_TopLeftAtXY()
    {
        var b = new PhysicsBody(PhysicsShape.Rect) { X = 10f, Y = 20f, Width = 80f, Height = 30f };
        var bb = b.BoundingBox;
        Assert.Equal(10f,  bb.Left,   precision: 4);
        Assert.Equal(20f,  bb.Top,    precision: 4);
        Assert.Equal(90f,  bb.Right,  precision: 4);
        Assert.Equal(50f,  bb.Bottom, precision: 4);
    }

    // ── Overlaps — Circle vs Circle ────────────────────────────────────────

    [Fact]
    public void Overlaps_CircleCircle_WhenOverlapping_ReturnsTrue()
    {
        var a = new PhysicsBody(PhysicsShape.Circle) { X = 0f, Y = 0f, Radius = 10f };
        var b = new PhysicsBody(PhysicsShape.Circle) { X = 5f, Y = 0f, Radius = 10f };
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_CircleCircle_WhenNotOverlapping_ReturnsFalse()
    {
        var a = new PhysicsBody(PhysicsShape.Circle) { X = 0f, Y = 0f, Radius = 5f };
        var b = new PhysicsBody(PhysicsShape.Circle) { X = 100f, Y = 0f, Radius = 5f };
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_CircleCircle_Symmetric()
    {
        var a = new PhysicsBody(PhysicsShape.Circle) { X = 0f, Y = 0f, Radius = 10f };
        var b = new PhysicsBody(PhysicsShape.Circle) { X = 5f, Y = 0f, Radius = 10f };
        Assert.Equal(a.Overlaps(b), b.Overlaps(a));
    }

    // ── Overlaps — Circle vs Rect ──────────────────────────────────────────

    [Fact]
    public void Overlaps_CircleRect_WhenCircleInsideRect_ReturnsTrue()
    {
        var circle = new PhysicsBody(PhysicsShape.Circle) { X = 50f, Y = 50f, Radius = 5f };
        var rect   = new PhysicsBody(PhysicsShape.Rect)   { X = 0f,  Y = 0f,  Width = 100f, Height = 100f };
        Assert.True(circle.Overlaps(rect));
    }

    [Fact]
    public void Overlaps_CircleRect_WhenCircleTouchesEdge_ReturnsTrue()
    {
        // Circle just overlaps the right edge of the rect
        var circle = new PhysicsBody(PhysicsShape.Circle) { X = 109f, Y = 50f, Radius = 10f };
        var rect   = new PhysicsBody(PhysicsShape.Rect)   { X = 0f,   Y = 0f,  Width = 100f, Height = 100f };
        Assert.True(circle.Overlaps(rect));
    }

    [Fact]
    public void Overlaps_CircleRect_WhenCircleFarAway_ReturnsFalse()
    {
        var circle = new PhysicsBody(PhysicsShape.Circle) { X = 500f, Y = 500f, Radius = 5f };
        var rect   = new PhysicsBody(PhysicsShape.Rect)   { X = 0f,   Y = 0f,  Width = 100f, Height = 100f };
        Assert.False(circle.Overlaps(rect));
    }

    // ── Overlaps — Rect vs Circle (reversed arg order) ────────────────────

    [Fact]
    public void Overlaps_RectCircle_DelegatesToCircleRect()
    {
        var circle = new PhysicsBody(PhysicsShape.Circle) { X = 50f, Y = 50f, Radius = 5f };
        var rect   = new PhysicsBody(PhysicsShape.Rect)   { X = 0f,  Y = 0f,  Width = 100f, Height = 100f };
        // Both orderings must agree
        Assert.Equal(circle.Overlaps(rect), rect.Overlaps(circle));
    }

    [Fact]
    public void Overlaps_RectCircle_WhenNotOverlapping_ReturnsFalse()
    {
        var circle = new PhysicsBody(PhysicsShape.Circle) { X = 500f, Y = 500f, Radius = 5f };
        var rect   = new PhysicsBody(PhysicsShape.Rect)   { X = 0f,   Y = 0f,  Width = 100f, Height = 100f };
        Assert.False(rect.Overlaps(circle));
    }

    // ── Overlaps — Rect vs Rect ────────────────────────────────────────────

    [Fact]
    public void Overlaps_RectRect_WhenOverlapping_ReturnsTrue()
    {
        var a = new PhysicsBody(PhysicsShape.Rect) { X = 0f,  Y = 0f,  Width = 50f, Height = 50f };
        var b = new PhysicsBody(PhysicsShape.Rect) { X = 25f, Y = 25f, Width = 50f, Height = 50f };
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_RectRect_WhenNotOverlapping_ReturnsFalse()
    {
        var a = new PhysicsBody(PhysicsShape.Rect) { X = 0f,   Y = 0f,  Width = 50f, Height = 50f };
        var b = new PhysicsBody(PhysicsShape.Rect) { X = 200f, Y = 200f, Width = 50f, Height = 50f };
        Assert.False(a.Overlaps(b));
    }

    // ── Reflect — guard clauses ────────────────────────────────────────────

    [Fact]
    public void Reflect_WhenThisIsRect_DoesNothing()
    {
        // Guard: Shape != Circle → early return
        var rect1 = new PhysicsBody(PhysicsShape.Rect) { VelocityX = 1f, VelocityY = 1f };
        var rect2 = new PhysicsBody(PhysicsShape.Rect) { X = 0f, Y = 0f, Width = 100f, Height = 100f };
        rect1.Reflect(rect2);
        Assert.Equal(1f, rect1.VelocityX);
        Assert.Equal(1f, rect1.VelocityY);
    }

    [Fact]
    public void Reflect_WhenOtherIsCircle_DoesNothing()
    {
        // Guard: this is Circle but rect.Shape != Rect → early return
        var circle1 = new PhysicsBody(PhysicsShape.Circle) { X = 50f, Y = 50f, Radius = 5f, VelocityX = 1f, VelocityY = 1f };
        var circle2 = new PhysicsBody(PhysicsShape.Circle) { X = 50f, Y = 50f, Radius = 5f };
        circle1.Reflect(circle2);
        Assert.Equal(1f, circle1.VelocityX);
        Assert.Equal(1f, circle1.VelocityY);
    }

    // ── Reflect — vertical axis (top/bottom) ───────────────────────────────

    [Fact]
    public void Reflect_BallHitsTopOfRect_FlipsVelocityY()
    {
        // Ball is just above the rect, moving downward.
        // It's barely overlapping the top edge (small vertical overlap, large horizontal overlap).
        // overlapTop < overlapLeft/Right → minV < minH → flip Y.
        var ball = new PhysicsBody(PhysicsShape.Circle)
        {
            X = 50f, Y = 19f, Radius = 8f,  // Y=19 → bottom edge at 27, brick top at 20 → overlapTop=7
            VelocityX = 0f, VelocityY = 200f
        };
        var brick = new PhysicsBody(PhysicsShape.Rect)
            { X = 0f, Y = 20f, Width = 100f, Height = 22f };

        ball.Reflect(brick);

        Assert.True(ball.VelocityY < 0f, "VelocityY should be flipped to negative");
        Assert.Equal(0f, ball.VelocityX, precision: 4);
    }

    // ── Reflect — horizontal axis (left/right side) ────────────────────────

    [Fact]
    public void Reflect_BallHitsSideOfRect_FlipsVelocityX()
    {
        // Ball hits from the left side: large vertical overlap, small horizontal overlap.
        // overlapLeft is tiny, overlapTop/Bottom are large → minH < minV → flip X.
        var ball = new PhysicsBody(PhysicsShape.Circle)
        {
            X = 3f, Y = 50f, Radius = 8f,   // X=3 → right edge at 11, rect left at 0 → overlapLeft=11
            // But rect is 100 wide, overlapRight = (0+100) - (3-8) = 105. minH = min(11,105)=11
            // rect top=0, height=200. overlapTop = (50+8) - 0 = 58. overlapBottom = (0+200)-(50-8)=158. minV=min(58,158)=58
            // minH(11) < minV(58) → flip X ✓
            VelocityX = 200f, VelocityY = 0f
        };
        var brick = new PhysicsBody(PhysicsShape.Rect)
            { X = 0f, Y = 0f, Width = 100f, Height = 200f };

        ball.Reflect(brick);

        Assert.True(ball.VelocityX < 0f, "VelocityX should be flipped to negative");
        Assert.Equal(0f, ball.VelocityY, precision: 4);
    }

    // ── ReflectOff — no overlap ────────────────────────────────────────────

    [Fact]
    public void ReflectOff_WhenNoOverlap_ReturnsFalse_VelocityUnchanged()
    {
        var ball  = new PhysicsBody(PhysicsShape.Circle) { X = 500f, Y = 500f, Radius = 5f, VelocityX = 1f, VelocityY = 1f };
        var brick = new PhysicsBody(PhysicsShape.Rect)   { X = 0f,   Y = 0f,   Width = 10f, Height = 10f };
        bool result = ball.ReflectOff(brick);
        Assert.False(result);
        Assert.Equal(1f, ball.VelocityX);
        Assert.Equal(1f, ball.VelocityY);
    }

    // ── ReflectOff — Circle vs Rect ────────────────────────────────────────

    [Fact]
    public void ReflectOff_CircleVsRect_WhenOverlapping_ReturnsTrue_AndReflects()
    {
        var ball = new PhysicsBody(PhysicsShape.Circle)
            { X = 50f, Y = 19f, Radius = 8f, VelocityX = 0f, VelocityY = 200f };
        var brick = new PhysicsBody(PhysicsShape.Rect)
            { X = 0f, Y = 20f, Width = 100f, Height = 22f };

        bool result = ball.ReflectOff(brick);

        Assert.True(result);
        Assert.True(ball.VelocityY < 0f);
    }

    // ── ReflectOff — Rect vs Circle ────────────────────────────────────────

    [Fact]
    public void ReflectOff_RectVsCircle_WhenOverlapping_ReturnsTrue()
    {
        // rect.ReflectOff(circle) — goes into the else branch with Reflect(this)
        // Reflect(this) → Shape != Circle → guard returns, velocity unchanged
        // But ReflectOff still returns true.
        var rect   = new PhysicsBody(PhysicsShape.Rect)   { X = 0f,  Y = 0f,  Width = 100f, Height = 100f, VelocityX = 1f, VelocityY = 1f };
        var circle = new PhysicsBody(PhysicsShape.Circle) { X = 50f, Y = 50f, Radius = 5f };

        bool result = rect.ReflectOff(circle);

        Assert.True(result);
    }

    // ── ReflectOff — Rect vs Rect ──────────────────────────────────────────

    [Fact]
    public void ReflectOff_RectVsRect_WhenOverlapping_ReturnsTrue()
    {
        var a = new PhysicsBody(PhysicsShape.Rect) { X = 0f, Y = 0f, Width = 50f, Height = 50f, VelocityX = 1f };
        var b = new PhysicsBody(PhysicsShape.Rect) { X = 25f, Y = 25f, Width = 50f, Height = 50f };
        bool result = a.ReflectOff(b);
        Assert.True(result);
    }

    // ── ReflectOff — Circle vs Circle ─────────────────────────────────────

    [Fact]
    public void ReflectOff_CircleCircle_WhenOverlapping_ReflectsVelocity()
    {
        var a = new PhysicsBody(PhysicsShape.Circle) { X = 0f, Y = 0f, Radius = 10f, VelocityX = 100f, VelocityY = 0f };
        var b = new PhysicsBody(PhysicsShape.Circle) { X = 5f, Y = 0f, Radius = 10f, VelocityX = 0f,   VelocityY = 0f };

        bool result = a.ReflectOff(b);

        Assert.True(result);
        // Ball was moving right (+X), hit ball to the right → velocity should reflect (go left)
        Assert.True(a.VelocityX < 100f, "VelocityX should have been reduced by reflection");
    }

    [Fact]
    public void ReflectOff_CircleCircle_SamePosition_SkipsReflect_StillReturnsTrue()
    {
        // len == 0 branch: both circles at the same position
        var a = new PhysicsBody(PhysicsShape.Circle) { X = 0f, Y = 0f, Radius = 10f, VelocityX = 50f, VelocityY = 50f };
        var b = new PhysicsBody(PhysicsShape.Circle) { X = 0f, Y = 0f, Radius = 10f };

        bool result = a.ReflectOff(b);

        Assert.True(result);
        // Velocity should be unchanged (len=0 branch skips reflect)
        Assert.Equal(50f, a.VelocityX, precision: 4);
        Assert.Equal(50f, a.VelocityY, precision: 4);
    }
}
