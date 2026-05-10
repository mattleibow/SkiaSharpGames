using SkiaSharp.Theatre;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

/// <summary>Tests for <see cref="Rigidbody2D"/> and <see cref="CollisionHit"/>.</summary>
public class Rigidbody2DTests
{
    [Fact]
    public void BounceX_WithRestitution_ScalesVelocity()
    {
        var body = new Rigidbody2D { VelocityX = 100f };
        body.BounceX(0.5f);
        Assert.Equal(-50f, body.VelocityX, precision: 3);
    }

    [Fact]
    public void BounceY_WithRestitution_ScalesVelocity()
    {
        var body = new Rigidbody2D { VelocityY = 200f };
        body.BounceY(0.8f);
        Assert.Equal(-160f, body.VelocityY, precision: 3);
    }

    [Fact]
    public void BounceX_WithZeroRestitution_StopsVelocity()
    {
        var body = new Rigidbody2D { VelocityX = 100f };
        body.BounceX(0f);
        Assert.Equal(0f, body.VelocityX, precision: 3);
    }

    [Fact]
    public void Bounce_WithDiagonalNormal_ReflectsCorrectly()
    {
        var body = new Rigidbody2D { VelocityX = 1f, VelocityY = 1f };
        body.Bounce(0f, -1f); // horizontal surface, normal pointing up
        Assert.Equal(1f, body.VelocityX, precision: 3);
        Assert.Equal(-1f, body.VelocityY, precision: 3);
    }

    [Fact]
    public void CollisionHit_IsHorizontal_WhenNormalXDominates()
    {
        var hit = new CollisionHit(1f, 0.5f, 5f);
        Assert.True(hit.IsHorizontal);
        Assert.False(hit.IsVertical);
    }

    [Fact]
    public void CollisionHit_IsVertical_WhenNormalYDominatesOrEqual()
    {
        var hitEqual = new CollisionHit(0.5f, 0.5f, 5f);
        Assert.True(hitEqual.IsVertical);

        var hitY = new CollisionHit(0f, 1f, 3f);
        Assert.True(hitY.IsVertical);
        Assert.False(hitY.IsHorizontal);
    }
}
