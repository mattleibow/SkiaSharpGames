using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class CollisionResolverTests
{
    [Fact]
    public void CircleCollider_BoundingBox_CentredOnActor()
    {
        var actor = new Actor { X = 100f, Y = 200f };
        var collider = new CircleCollider { Radius = 10f };

        var bounds = collider.BoundingBox(actor.X, actor.Y);

        Assert.Equal(90f, bounds.Left, precision: 4);
        Assert.Equal(190f, bounds.Top, precision: 4);
        Assert.Equal(110f, bounds.Right, precision: 4);
        Assert.Equal(210f, bounds.Bottom, precision: 4);
    }

    [Fact]
    public void RectCollider_WorldRect_IsCentredOnActor()
    {
        var actor = new Actor { X = 50f, Y = 30f };
        var collider = new RectCollider { Width = 20f, Height = 10f };

        var bounds = collider.WorldRect(actor.X, actor.Y);

        Assert.Equal(40f, bounds.Left, precision: 4);
        Assert.Equal(25f, bounds.Top, precision: 4);
        Assert.Equal(60f, bounds.Right, precision: 4);
        Assert.Equal(35f, bounds.Bottom, precision: 4);
    }

    [Fact]
    public void Rigidbody2D_Step_MovesActor()
    {
        var actor = new Actor { X = 10f, Y = 20f };
        var body = new Rigidbody2D();
        body.SetVelocity(100f, -50f);

        body.Step(actor, 0.5f);

        Assert.Equal(60f, actor.X, precision: 4);
        Assert.Equal(-5f, actor.Y, precision: 4);
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
        var a = new Actor { X = 0f, Y = 0f, Collider = new CircleCollider { Radius = 10f } };
        var b = new Actor { X = 5f, Y = 0f, Collider = new CircleCollider { Radius = 10f } };

        Assert.True(CollisionResolver.Overlaps(a, b));
        Assert.True(CollisionResolver.TryGetHit(a, b, out var hit));
        Assert.True(hit.NormalX < 0f);
    }

    [Fact]
    public void TryGetHit_CircleCircle_SamePosition_UsesFallbackNormal()
    {
        var a = new Actor { X = 0f, Y = 0f, Collider = new CircleCollider { Radius = 10f } };
        var b = new Actor { X = 0f, Y = 0f, Collider = new CircleCollider { Radius = 10f } };

        var result = CollisionResolver.TryGetHit(a, b, out var hit);

        Assert.True(result);
        Assert.Equal(0f, hit.NormalX, precision: 4);
        Assert.Equal(-1f, hit.NormalY, precision: 4);
    }

    [Fact]
    public void Overlaps_CircleRect_WhenCircleInsideRect_ReturnsTrue()
    {
        var circleOwner = new Actor { X = 50f, Y = 50f, Collider = new CircleCollider { Radius = 5f } };
        var rectOwner = new Actor { X = 50f, Y = 50f, Collider = new RectCollider { Width = 100f, Height = 100f } };

        Assert.True(CollisionResolver.Overlaps(circleOwner, rectOwner));
    }

    [Fact]
    public void TryGetHit_CircleRect_FromTop_ReturnsUpwardNormal()
    {
        var ball = new Actor { X = 50f, Y = 19f, Collider = new CircleCollider { Radius = 8f } };
        var brick = new Actor { X = 50f, Y = 31f, Collider = new RectCollider { Width = 100f, Height = 22f } };

        var result = CollisionResolver.TryGetHit(ball, brick, out var hit);

        Assert.True(result);
        Assert.True(hit.IsVertical);
        Assert.Equal(-1f, hit.NormalY, precision: 4);
    }

    [Fact]
    public void TryGetHit_CircleRect_FromSide_ReturnsHorizontalNormal()
    {
        var ball = new Actor { X = 3f, Y = 50f, Collider = new CircleCollider { Radius = 8f } };
        var wall = new Actor { X = 50f, Y = 100f, Collider = new RectCollider { Width = 100f, Height = 200f } };

        var result = CollisionResolver.TryGetHit(ball, wall, out var hit);

        Assert.True(result);
        Assert.True(hit.IsHorizontal);
        Assert.Equal(-1f, hit.NormalX, precision: 4);
    }

    [Fact]
    public void TryGetHit_CircleRect_WhenSeparated_ReturnsFalse()
    {
        var circleOwner = new Actor { X = 500f, Y = 500f, Collider = new CircleCollider { Radius = 5f } };
        var rectOwner = new Actor { X = 50f, Y = 50f, Collider = new RectCollider { Width = 100f, Height = 100f } };

        var result = CollisionResolver.TryGetHit(circleOwner, rectOwner, out _);

        Assert.False(result);
    }

    [Fact]
    public void Overlaps_RectCircle_AgreesWithCircleRect()
    {
        var circle = new CircleCollider { Radius = 5f };
        var rect = new RectCollider { Width = 100f, Height = 100f };
        var circleOwner = new Actor { X = 50f, Y = 50f, Collider = circle };
        var rectOwner = new Actor { X = 50f, Y = 50f, Collider = rect };

        Assert.Equal(
            CollisionResolver.Overlaps(circleOwner, rectOwner),
            CollisionResolver.Overlaps(rectOwner, circleOwner));
    }

    [Fact]
    public void TryGetHit_RectVsCircle_InvertsNormalForFirstCollider()
    {
        var circleOwner = new Actor { X = 3f, Y = 50f, Collider = new CircleCollider { Radius = 8f } };
        var rectOwner = new Actor { X = 50f, Y = 100f, Collider = new RectCollider { Width = 100f, Height = 200f } };

        var result = CollisionResolver.TryGetHit(rectOwner, circleOwner, out var hit);

        Assert.True(result);
        Assert.True(hit.NormalX > 0f);
    }

    [Fact]
    public void Overlaps_RectRect_WhenSeparated_ReturnsFalse()
    {
        var a = new Actor { X = 0f, Y = 0f, Collider = new RectCollider { Width = 50f, Height = 50f } };
        var b = new Actor { X = 200f, Y = 200f, Collider = new RectCollider { Width = 50f, Height = 50f } };

        Assert.False(CollisionResolver.Overlaps(a, b));
    }

    [Fact]
    public void TryGetHit_RectRect_WhenOverlapping_ReturnsCollision()
    {
        var a = new Actor { X = 20f, Y = 20f, Collider = new RectCollider { Width = 50f, Height = 50f } };
        var b = new Actor { X = 40f, Y = 25f, Collider = new RectCollider { Width = 50f, Height = 50f } };

        var result = CollisionResolver.TryGetHit(a, b, out var hit);

        Assert.True(result);
        Assert.True(hit.Penetration > 0f);
    }

    [Fact]
    public void TryGetHit_RectRect_WhenVerticalOverlapIsSmaller_ReturnsVerticalNormal()
    {
        var a = new Actor { X = 20f, Y = 20f, Collider = new RectCollider { Width = 50f, Height = 50f } };
        var b = new Actor { X = 20f, Y = 55f, Collider = new RectCollider { Width = 50f, Height = 50f } };

        var result = CollisionResolver.TryGetHit(a, b, out var hit);

        Assert.True(result);
        Assert.True(hit.IsVertical);
        Assert.Equal(-1f, hit.NormalY, precision: 4);
    }

    [Fact]
    public void ReflectOff_CircleRect_BouncesVelocity()
    {
        var body = new Rigidbody2D { VelocityY = 200f };
        var ball = new Actor { X = 50f, Y = 19f, Collider = new CircleCollider { Radius = 8f }, Rigidbody = body };
        var brick = new Actor { X = 50f, Y = 31f, Collider = new RectCollider { Width = 100f, Height = 22f } };

        var result = ball.BounceOff(brick);

        Assert.True(result);
        Assert.True(body.VelocityY < 0f);
    }

    [Fact]
    public void Reflect_CircleRect_BouncesVelocity()
    {
        var body = new Rigidbody2D { VelocityY = 200f };
        var ball = new Actor { X = 50f, Y = 19f, Collider = new CircleCollider { Radius = 8f }, Rigidbody = body };
        var brick = new Actor { X = 50f, Y = 31f, Collider = new RectCollider { Width = 100f, Height = 22f } };

        ball.BounceOff(brick);

        Assert.True(body.VelocityY < 0f);
    }

    [Fact]
    public void CircleVsRectWall_BouncesLikeOldBounds()
    {
        // A ball heading left into a thick wall actor on the left edge — same
        // scenario that ResolveBounds used to handle, now done via TryGetHit.
        var circle = new CircleCollider { Radius = 8f };
        var body = new Rigidbody2D { VelocityX = -120f };
        var ball = new Actor { X = 5f, Y = 50f, Collider = circle };

        // Wall sits at x = -50 (centre), width = 100, so its right edge is at x = 0.
        var wallCollider = new RectCollider { Width = 100f, Height = 200f };
        var wall = new Actor { X = -50f, Y = 50f, Collider = wallCollider };

        Assert.True(CollisionResolver.TryGetHit(ball, wall, out var hit));
        body.Bounce(hit);
        Assert.True(body.VelocityX > 0f, "Ball should bounce rightward off the left wall");
    }

    // ── RectCollider.BoundingBox ──────────────────────────────────────────

    [Fact]
    public void RectCollider_BoundingBox_MatchesWorldRect()
    {
        var actor = new Actor { X = 100f, Y = 50f };
        var collider = new RectCollider { Width = 40f, Height = 20f };

        var worldRect = collider.WorldRect(actor.X, actor.Y);
        var boundingBox = collider.BoundingBox(actor.X, actor.Y);

        Assert.Equal(worldRect.Left, boundingBox.Left, precision: 4);
        Assert.Equal(worldRect.Top, boundingBox.Top, precision: 4);
        Assert.Equal(worldRect.Right, boundingBox.Right, precision: 4);
        Assert.Equal(worldRect.Bottom, boundingBox.Bottom, precision: 4);
    }

    [Fact]
    public void RectCollider_BoundingBox_WithOffset_IsShifted()
    {
        var actor = new Actor { X = 100f, Y = 100f };
        var collider = new RectCollider { Width = 20f, Height = 10f, OffsetX = 5f, OffsetY = -5f };

        var box = collider.BoundingBox(actor.X, actor.Y);

        Assert.Equal(95f, box.Left, precision: 4);
        Assert.Equal(90f, box.Top, precision: 4);
        Assert.Equal(115f, box.Right, precision: 4);
        Assert.Equal(100f, box.Bottom, precision: 4);
    }

    [Fact]
    public void CircleCollider_WithOffset_WorldCenterIsShifted()
    {
        var actor = new Actor { X = 100f, Y = 100f };
        var collider = new CircleCollider { Radius = 5f, OffsetX = 10f, OffsetY = -5f };

        var (cx, cy) = collider.WorldCenter(actor.X, actor.Y);

        Assert.Equal(110f, cx, precision: 4);
        Assert.Equal(95f, cy, precision: 4);
    }
}
