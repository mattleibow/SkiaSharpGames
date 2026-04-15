using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

public class CollisionResolverTests
{
    private sealed class TestEntity : Entity { }

    [Fact]
    public void CircleCollider_BoundingBox_CentredOnEntity()
    {
        var entity = new TestEntity { X = 100f, Y = 200f };
        var collider = new CircleCollider { Radius = 10f };

        var bounds = collider.BoundingBox(entity);

        Assert.Equal(90f, bounds.Left, precision: 4);
        Assert.Equal(190f, bounds.Top, precision: 4);
        Assert.Equal(110f, bounds.Right, precision: 4);
        Assert.Equal(210f, bounds.Bottom, precision: 4);
    }

    [Fact]
    public void RectCollider_WorldRect_IsCentredOnEntity()
    {
        var entity = new TestEntity { X = 50f, Y = 30f };
        var collider = new RectCollider { Width = 20f, Height = 10f };

        var bounds = collider.WorldRect(entity);

        Assert.Equal(40f, bounds.Left, precision: 4);
        Assert.Equal(25f, bounds.Top, precision: 4);
        Assert.Equal(60f, bounds.Right, precision: 4);
        Assert.Equal(35f, bounds.Bottom, precision: 4);
    }

    [Fact]
    public void Rigidbody2D_Step_MovesEntity()
    {
        var entity = new TestEntity { X = 10f, Y = 20f };
        var body = new Rigidbody2D();
        body.SetVelocity(100f, -50f);

        body.Step(entity, 0.5f);

        Assert.Equal(60f, entity.X, precision: 4);
        Assert.Equal(-5f, entity.Y, precision: 4);
    }

    [Fact]
    public void Rigidbody2D_BounceX_FlipsHorizontalVelocity()
    {
        var body = new Rigidbody2D { VelocityX = 120f, VelocityY = 5f };

        body.BounceX();

        Assert.Equal(-120f, body.VelocityX, precision: 4);
        Assert.Equal(5f, body.VelocityY, precision: 4);
    }

    [Fact]
    public void Rigidbody2D_Bounce_WithNormal_ReflectsVelocity()
    {
        var body = new Rigidbody2D { VelocityX = 0f, VelocityY = 200f };

        body.Bounce(0f, -1f);

        Assert.Equal(0f, body.VelocityX, precision: 4);
        Assert.Equal(-200f, body.VelocityY, precision: 4);
    }

    [Fact]
    public void Rigidbody2D_Bounce_WithZeroNormal_IsNoOp()
    {
        var body = new Rigidbody2D { VelocityX = 10f, VelocityY = 20f };

        body.Bounce(0f, 0f);

        Assert.Equal(10f, body.VelocityX, precision: 4);
        Assert.Equal(20f, body.VelocityY, precision: 4);
    }

    [Fact]
    public void Rigidbody2D_Bounce_WhenMovingAwayFromSurface_IsNoOp()
    {
        var body = new Rigidbody2D { VelocityX = 0f, VelocityY = -100f };

        body.Bounce(0f, -1f);

        Assert.Equal(-100f, body.VelocityY, precision: 4);
    }

    [Fact]
    public void Rigidbody2D_AddVelocity_AndStop_WorkAsExpected()
    {
        var body = new Rigidbody2D();

        body.AddVelocity(3f, 4f);
        Assert.Equal(5f, body.Speed, precision: 4);

        body.Stop();
        Assert.Equal(0f, body.Speed, precision: 4);
    }

    [Fact]
    public void Overlaps_CircleCircle_WhenOverlapping_ReturnsTrue()
    {
        var a = new TestEntity { X = 0f, Y = 0f };
        var b = new TestEntity { X = 5f, Y = 0f };
        var aCollider = new CircleCollider { Radius = 10f };
        var bCollider = new CircleCollider { Radius = 10f };

        Assert.True(CollisionResolver.Overlaps(a, aCollider, b, bCollider));
        Assert.True(CollisionResolver.TryGetHit(a, aCollider, b, bCollider, out var hit));
        Assert.True(hit.NormalX < 0f);
    }

    [Fact]
    public void TryGetHit_CircleCircle_SamePosition_UsesFallbackNormal()
    {
        var a = new TestEntity { X = 0f, Y = 0f };
        var b = new TestEntity { X = 0f, Y = 0f };
        var aCollider = new CircleCollider { Radius = 10f };
        var bCollider = new CircleCollider { Radius = 10f };

        var result = CollisionResolver.TryGetHit(a, aCollider, b, bCollider, out var hit);

        Assert.True(result);
        Assert.Equal(0f, hit.NormalX, precision: 4);
        Assert.Equal(-1f, hit.NormalY, precision: 4);
    }

    [Fact]
    public void Overlaps_CircleRect_WhenCircleInsideRect_ReturnsTrue()
    {
        var circleOwner = new TestEntity { X = 50f, Y = 50f };
        var rectOwner = new TestEntity { X = 50f, Y = 50f };
        var circle = new CircleCollider { Radius = 5f };
        var rect = new RectCollider { Width = 100f, Height = 100f };

        Assert.True(CollisionResolver.Overlaps(circleOwner, circle, rectOwner, rect));
    }

    [Fact]
    public void TryGetHit_CircleRect_FromTop_ReturnsUpwardNormal()
    {
        var ball = new TestEntity { X = 50f, Y = 19f };
        var brick = new TestEntity { X = 50f, Y = 31f };
        var circle = new CircleCollider { Radius = 8f };
        var rect = new RectCollider { Width = 100f, Height = 22f };

        var result = CollisionResolver.TryGetHit(ball, circle, brick, rect, out var hit);

        Assert.True(result);
        Assert.True(hit.IsVertical);
        Assert.Equal(-1f, hit.NormalY, precision: 4);
    }

    [Fact]
    public void TryGetHit_CircleRect_FromSide_ReturnsHorizontalNormal()
    {
        var ball = new TestEntity { X = 3f, Y = 50f };
        var wall = new TestEntity { X = 50f, Y = 100f };
        var circle = new CircleCollider { Radius = 8f };
        var rect = new RectCollider { Width = 100f, Height = 200f };

        var result = CollisionResolver.TryGetHit(ball, circle, wall, rect, out var hit);

        Assert.True(result);
        Assert.True(hit.IsHorizontal);
        Assert.Equal(-1f, hit.NormalX, precision: 4);
    }

    [Fact]
    public void TryGetHit_CircleRect_WhenSeparated_ReturnsFalse()
    {
        var circleOwner = new TestEntity { X = 500f, Y = 500f };
        var rectOwner = new TestEntity { X = 50f, Y = 50f };
        var circle = new CircleCollider { Radius = 5f };
        var rect = new RectCollider { Width = 100f, Height = 100f };

        var result = CollisionResolver.TryGetHit(circleOwner, circle, rectOwner, rect, out _);

        Assert.False(result);
    }

    [Fact]
    public void Overlaps_RectCircle_AgreesWithCircleRect()
    {
        var circleOwner = new TestEntity { X = 50f, Y = 50f };
        var rectOwner = new TestEntity { X = 50f, Y = 50f };
        var circle = new CircleCollider { Radius = 5f };
        var rect = new RectCollider { Width = 100f, Height = 100f };

        Assert.Equal(
            CollisionResolver.Overlaps(circleOwner, circle, rectOwner, rect),
            CollisionResolver.Overlaps(rectOwner, rect, circleOwner, circle));
    }

    [Fact]
    public void TryGetHit_RectVsCircle_InvertsNormalForFirstCollider()
    {
        var circleOwner = new TestEntity { X = 3f, Y = 50f };
        var rectOwner = new TestEntity { X = 50f, Y = 100f };
        var circle = new CircleCollider { Radius = 8f };
        var rect = new RectCollider { Width = 100f, Height = 200f };

        var result = CollisionResolver.TryGetHit(rectOwner, rect, circleOwner, circle, out var hit);

        Assert.True(result);
        Assert.True(hit.NormalX > 0f);
    }

    [Fact]
    public void Overlaps_RectRect_WhenSeparated_ReturnsFalse()
    {
        var a = new TestEntity { X = 0f, Y = 0f };
        var b = new TestEntity { X = 200f, Y = 200f };
        var rectA = new RectCollider { Width = 50f, Height = 50f };
        var rectB = new RectCollider { Width = 50f, Height = 50f };

        Assert.False(CollisionResolver.Overlaps(a, rectA, b, rectB));
    }

    [Fact]
    public void TryGetHit_RectRect_WhenOverlapping_ReturnsCollision()
    {
        var a = new TestEntity { X = 20f, Y = 20f };
        var b = new TestEntity { X = 40f, Y = 25f };
        var rectA = new RectCollider { Width = 50f, Height = 50f };
        var rectB = new RectCollider { Width = 50f, Height = 50f };

        var result = CollisionResolver.TryGetHit(a, rectA, b, rectB, out var hit);

        Assert.True(result);
        Assert.True(hit.Penetration > 0f);
    }

    [Fact]
    public void TryGetHit_RectRect_WhenVerticalOverlapIsSmaller_ReturnsVerticalNormal()
    {
        var a = new TestEntity { X = 20f, Y = 20f };
        var b = new TestEntity { X = 20f, Y = 55f };
        var rectA = new RectCollider { Width = 50f, Height = 50f };
        var rectB = new RectCollider { Width = 50f, Height = 50f };

        var result = CollisionResolver.TryGetHit(a, rectA, b, rectB, out var hit);

        Assert.True(result);
        Assert.True(hit.IsVertical);
        Assert.Equal(-1f, hit.NormalY, precision: 4);
    }

    [Fact]
    public void ReflectOff_CircleRect_BouncesVelocity()
    {
        var ball = new TestEntity { X = 50f, Y = 19f };
        var brick = new TestEntity { X = 50f, Y = 31f };
        var circle = new CircleCollider { Radius = 8f };
        var rect = new RectCollider { Width = 100f, Height = 22f };
        var body = new Rigidbody2D { VelocityY = 200f };

        var result = CollisionResolver.ReflectOff(ball, circle, body, brick, rect);

        Assert.True(result);
        Assert.True(body.VelocityY < 0f);
    }

    [Fact]
    public void Reflect_CircleRect_BouncesVelocity()
    {
        var ball = new TestEntity { X = 50f, Y = 19f };
        var brick = new TestEntity { X = 50f, Y = 31f };
        var circle = new CircleCollider { Radius = 8f };
        var rect = new RectCollider { Width = 100f, Height = 22f };
        var body = new Rigidbody2D { VelocityY = 200f };

        CollisionResolver.Reflect(ball, circle, body, brick, rect);

        Assert.True(body.VelocityY < 0f);
    }

    [Fact]
    public void CircleVsRectWall_BouncesLikeOldBounds()
    {
        // A ball heading left into a thick wall entity on the left edge — same
        // scenario that ResolveBounds used to handle, now done via TryGetHit.
        var ball = new TestEntity { X = 5f, Y = 50f };
        var circle = new CircleCollider { Radius = 8f };
        var body = new Rigidbody2D { VelocityX = -120f };

        // Wall sits at x = -50 (centre), width = 100, so its right edge is at x = 0.
        var wall = new TestEntity { X = -50f, Y = 50f };
        var wallCollider = new RectCollider { Width = 100f, Height = 200f };

        Assert.True(CollisionResolver.TryGetHit(ball, circle, wall, wallCollider, out var hit));
        body.Bounce(hit);
        Assert.True(body.VelocityX > 0f, "Ball should bounce rightward off the left wall");
    }

    // ── RectCollider.BoundingBox ──────────────────────────────────────────

    [Fact]
    public void RectCollider_BoundingBox_MatchesWorldRect()
    {
        var entity = new TestEntity { X = 100f, Y = 50f };
        var collider = new RectCollider { Width = 40f, Height = 20f };

        var worldRect = collider.WorldRect(entity);
        var boundingBox = collider.BoundingBox(entity);

        Assert.Equal(worldRect.Left, boundingBox.Left, precision: 4);
        Assert.Equal(worldRect.Top, boundingBox.Top, precision: 4);
        Assert.Equal(worldRect.Right, boundingBox.Right, precision: 4);
        Assert.Equal(worldRect.Bottom, boundingBox.Bottom, precision: 4);
    }

    [Fact]
    public void RectCollider_BoundingBox_WithOffset_IsShifted()
    {
        var entity = new TestEntity { X = 100f, Y = 100f };
        var collider = new RectCollider { Width = 20f, Height = 10f, OffsetX = 5f, OffsetY = -5f };

        var box = collider.BoundingBox(entity);

        Assert.Equal(95f, box.Left, precision: 4);
        Assert.Equal(90f, box.Top, precision: 4);
        Assert.Equal(115f, box.Right, precision: 4);
        Assert.Equal(100f, box.Bottom, precision: 4);
    }

    [Fact]
    public void CircleCollider_WithOffset_WorldCenterIsShifted()
    {
        var entity = new TestEntity { X = 100f, Y = 100f };
        var collider = new CircleCollider { Radius = 5f, OffsetX = 10f, OffsetY = -5f };

        var (cx, cy) = collider.WorldCenter(entity);

        Assert.Equal(110f, cx, precision: 4);
        Assert.Equal(95f, cy, precision: 4);
    }
}
