using SkiaSharp;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

public class EntityTests
{
    // ── Position & World Position ─────────────────────────────────────

    [Fact]
    public void RootEntity_WorldPosition_EqualsLocalPosition()
    {
        var entity = new Entity { X = 10f, Y = 20f };
        Assert.Equal(10f, entity.WorldX);
        Assert.Equal(20f, entity.WorldY);
    }

    [Fact]
    public void ChildEntity_WorldPosition_IsParentPlusLocal()
    {
        var parent = new Entity { X = 100f, Y = 200f };
        var child = new Entity { X = 10f, Y = 20f };
        parent.AddChild(child);

        Assert.Equal(110f, child.WorldX);
        Assert.Equal(220f, child.WorldY);
    }

    [Fact]
    public void NestedChildren_WorldPosition_AccumulatesAllAncestors()
    {
        var root = new Entity { X = 10f, Y = 10f };
        var mid = new Entity { X = 20f, Y = 20f };
        var leaf = new Entity { X = 5f, Y = 5f };
        root.AddChild(mid);
        mid.AddChild(leaf);

        Assert.Equal(35f, leaf.WorldX);
        Assert.Equal(35f, leaf.WorldY);
    }

    [Fact]
    public void ParentPositionChange_PropagatesWorldToChildren()
    {
        var parent = new Entity { X = 0f, Y = 0f };
        var child = new Entity { X = 10f, Y = 10f };
        parent.AddChild(child);

        Assert.Equal(10f, child.WorldX);

        parent.X = 50f;
        Assert.Equal(60f, child.WorldX);
        Assert.Equal(10f, child.WorldY);
    }

    [Fact]
    public void ChildPositionChange_UpdatesOwnWorldOnly()
    {
        var parent = new Entity { X = 100f, Y = 200f };
        var child1 = new Entity { X = 10f, Y = 10f };
        var child2 = new Entity { X = 20f, Y = 20f };
        parent.AddChild(child1);
        parent.AddChild(child2);

        child1.X = 50f;
        Assert.Equal(150f, child1.WorldX);
        Assert.Equal(120f, child2.WorldX); // unchanged
    }

    // ── Rotation ──────────────────────────────────────────────────────

    [Fact]
    public void RootEntity_WorldRotation_EqualsLocalRotation()
    {
        var entity = new Entity { Rotation = MathF.PI / 4f };
        Assert.Equal(MathF.PI / 4f, entity.WorldRotation);
    }

    [Fact]
    public void ParentRotation_AffectsChildWorldPosition()
    {
        var parent = new Entity { X = 0f, Y = 0f, Rotation = MathF.PI / 2f }; // 90°
        var child = new Entity { X = 10f, Y = 0f };
        parent.AddChild(child);

        // 90° rotation: (10, 0) → (0, 10)
        Assert.Equal(0f, child.WorldX, 1e-4f);
        Assert.Equal(10f, child.WorldY, 1e-4f);
    }

    [Fact]
    public void ParentRotation_AccumulatesChildRotation()
    {
        var parent = new Entity { Rotation = MathF.PI / 4f };
        var child = new Entity { Rotation = MathF.PI / 4f };
        parent.AddChild(child);

        Assert.Equal(MathF.PI / 2f, child.WorldRotation, 1e-5f);
    }

    [Fact]
    public void NoParentRotation_UsesSimpleAddition()
    {
        // Tests the fast path (no trig)
        var parent = new Entity { X = 100f, Y = 100f }; // rotation = 0
        var child = new Entity { X = 50f, Y = 50f };
        parent.AddChild(child);

        Assert.Equal(150f, child.WorldX);
        Assert.Equal(150f, child.WorldY);
        Assert.Equal(0f, child.WorldRotation);
    }

    // ── Children ──────────────────────────────────────────────────────

    [Fact]
    public void AddChild_SetsParentAndRecalculatesWorld()
    {
        var parent = new Entity { X = 100f, Y = 100f };
        var child = new Entity { X = 10f, Y = 10f };

        parent.AddChild(child);

        Assert.Same(parent, child.Parent);
        Assert.Single(parent.Children);
        Assert.Equal(110f, child.WorldX);
    }

    [Fact]
    public void AddChild_RemovesFromPreviousParent()
    {
        var parent1 = new Entity();
        var parent2 = new Entity { X = 50f, Y = 50f };
        var child = new Entity { X = 10f, Y = 10f };

        parent1.AddChild(child);
        Assert.Single(parent1.Children);

        parent2.AddChild(child);
        Assert.Empty(parent1.Children);
        Assert.Single(parent2.Children);
        Assert.Same(parent2, child.Parent);
        Assert.Equal(60f, child.WorldX);
    }

    [Fact]
    public void RemoveChild_ClearsParentAndRecalculates()
    {
        var parent = new Entity { X = 100f, Y = 100f };
        var child = new Entity { X = 10f, Y = 10f };
        parent.AddChild(child);

        parent.RemoveChild(child);

        Assert.Null(child.Parent);
        Assert.Empty(parent.Children);
        Assert.Equal(10f, child.WorldX); // now root
    }

    [Fact]
    public void RemoveInactiveChildren_RemovesOnlyInactive()
    {
        var parent = new Entity();
        var active = new Entity { Active = true };
        var inactive = new Entity { Active = false };
        parent.AddChild(active);
        parent.AddChild(inactive);

        int removed = parent.RemoveInactiveChildren();

        Assert.Equal(1, removed);
        Assert.Single(parent.Children);
        Assert.Same(active, parent.Children[0]);
        Assert.Null(inactive.Parent);
    }

    [Fact]
    public void ChildCount_ReflectsChildren()
    {
        var parent = new Entity();
        Assert.Equal(0, parent.ChildCount);

        parent.AddChild(new Entity());
        parent.AddChild(new Entity());
        Assert.Equal(2, parent.ChildCount);
    }

    // ── Update ────────────────────────────────────────────────────────

    [Fact]
    public void Update_StepsRigidbody()
    {
        var entity = new Entity
        {
            X = 0f, Y = 0f,
            Rigidbody = new Rigidbody2D { VelocityX = 100f, VelocityY = 50f }
        };

        entity.Update(0.5f);

        Assert.Equal(50f, entity.X);
        Assert.Equal(25f, entity.Y);
    }

    [Fact]
    public void Update_RecursesIntoActiveChildren()
    {
        var parent = new Entity();
        var child = new Entity
        {
            Rigidbody = new Rigidbody2D { VelocityX = 10f }
        };
        parent.AddChild(child);

        parent.Update(1f);

        Assert.Equal(10f, child.X);
    }

    [Fact]
    public void Update_SkipsInactiveEntities()
    {
        var entity = new Entity
        {
            Active = false,
            Rigidbody = new Rigidbody2D { VelocityX = 100f }
        };

        entity.Update(1f);

        Assert.Equal(0f, entity.X);
    }

    [Fact]
    public void Update_CallsOnUpdate()
    {
        var entity = new TestEntity();
        entity.Update(0.5f);
        Assert.True(entity.OnUpdateCalled);
        Assert.Equal(0.5f, entity.LastDeltaTime);
    }

    // ── Draw ──────────────────────────────────────────────────────────

    [Fact]
    public void Draw_SkipsInvisibleEntity()
    {
        var entity = new Entity { Visible = false, Sprite = new TrackingSprite() };
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        entity.Draw(surface.Canvas);
        Assert.False(((TrackingSprite)entity.Sprite).DrawCalled);
    }

    [Fact]
    public void Draw_SkipsInactiveEntity()
    {
        var entity = new Entity { Active = false, Sprite = new TrackingSprite() };
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        entity.Draw(surface.Canvas);
        Assert.False(((TrackingSprite)entity.Sprite).DrawCalled);
    }

    [Fact]
    public void Draw_DrawsSprite()
    {
        var sprite = new TrackingSprite();
        var entity = new Entity { Sprite = sprite };
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        entity.Draw(surface.Canvas);
        Assert.True(sprite.DrawCalled);
    }

    [Fact]
    public void Draw_RecursesIntoChildren()
    {
        var childSprite = new TrackingSprite();
        var parent = new Entity();
        var child = new Entity { Sprite = childSprite };
        parent.AddChild(child);

        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        parent.Draw(surface.Canvas);
        Assert.True(childSprite.DrawCalled);
    }

    // ── Collision Helpers ─────────────────────────────────────────────

    [Fact]
    public void Overlaps_ReturnsTrueForOverlappingEntities()
    {
        var a = new Entity { X = 0f, Y = 0f, Collider = new CircleCollider { Radius = 10f } };
        var b = new Entity { X = 5f, Y = 0f, Collider = new CircleCollider { Radius = 10f } };

        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_ReturnsFalseForDistantEntities()
    {
        var a = new Entity { X = 0f, Y = 0f, Collider = new CircleCollider { Radius = 5f } };
        var b = new Entity { X = 100f, Y = 0f, Collider = new CircleCollider { Radius = 5f } };

        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_ReturnsFalseWithoutCollider()
    {
        var a = new Entity { X = 0f, Y = 0f };
        var b = new Entity { X = 0f, Y = 0f, Collider = new CircleCollider { Radius = 10f } };

        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void BounceOff_ReflectsVelocity()
    {
        var ball = new Entity
        {
            X = 5f, Y = 0f,
            Collider = new CircleCollider { Radius = 10f },
            Rigidbody = new Rigidbody2D { VelocityX = -100f }
        };
        var wall = new Entity
        {
            X = 0f, Y = 0f,
            Collider = new RectCollider { Width = 2f, Height = 100f }
        };

        bool bounced = ball.BounceOff(wall);
        Assert.True(bounced);
        Assert.True(ball.Rigidbody.VelocityX > 0); // reflected
    }

    [Fact]
    public void BounceOff_ReturnsFalseWithoutRigidbody()
    {
        var a = new Entity { Collider = new CircleCollider { Radius = 10f } };
        var b = new Entity { Collider = new CircleCollider { Radius = 10f } };

        Assert.False(a.BounceOff(b));
    }

    // ── WorldBoundingBox ──────────────────────────────────────────────

    [Fact]
    public void WorldBoundingBox_NullWithoutCollider()
    {
        var entity = new Entity { X = 10f, Y = 10f };
        Assert.Null(entity.WorldBoundingBox);
    }

    [Fact]
    public void WorldBoundingBox_UsesWorldPosition()
    {
        var parent = new Entity { X = 100f, Y = 100f };
        var child = new Entity
        {
            X = 10f, Y = 10f,
            Collider = new RectCollider { Width = 20f, Height = 20f }
        };
        parent.AddChild(child);

        var bb = child.WorldBoundingBox;
        Assert.NotNull(bb);
        // World pos is (110, 110), rect centered → (100, 100) to (120, 120)
        Assert.Equal(100f, bb.Value.Left);
        Assert.Equal(100f, bb.Value.Top);
        Assert.Equal(120f, bb.Value.Right);
        Assert.Equal(120f, bb.Value.Bottom);
    }

    [Fact]
    public void WorldBoundingBox_WithRotation_ReturnsConservativeAABB()
    {
        var entity = new Entity
        {
            X = 0f, Y = 0f,
            Rotation = MathF.PI / 4f, // 45°
            Collider = new RectCollider { Width = 10f, Height = 10f }
        };

        var bb = entity.WorldBoundingBox;
        Assert.NotNull(bb);
        // 45° rotated 10x10 rect has AABB larger than 10x10
        Assert.True(bb.Value.Width > 10f);
        Assert.True(bb.Value.Height > 10f);
    }

    // ── ChildrenBoundingBox ───────────────────────────────────────────

    [Fact]
    public void ChildrenBoundingBox_NullWithNoChildren()
    {
        var entity = new Entity();
        Assert.Null(entity.ChildrenBoundingBox);
    }

    [Fact]
    public void ChildrenBoundingBox_EnclosesAllActiveChildren()
    {
        var parent = new Entity();
        parent.AddChild(new Entity
        {
            X = 0f, Y = 0f,
            Collider = new RectCollider { Width = 10f, Height = 10f }
        });
        parent.AddChild(new Entity
        {
            X = 100f, Y = 0f,
            Collider = new RectCollider { Width = 10f, Height = 10f }
        });

        var bb = parent.ChildrenBoundingBox;
        Assert.NotNull(bb);
        Assert.Equal(-5f, bb.Value.Left);
        Assert.Equal(105f, bb.Value.Right);
    }

    [Fact]
    public void ChildrenBoundingBox_SkipsInactiveChildren()
    {
        var parent = new Entity();
        parent.AddChild(new Entity
        {
            X = 0f, Y = 0f,
            Collider = new RectCollider { Width = 10f, Height = 10f }
        });
        parent.AddChild(new Entity
        {
            X = 1000f, Y = 0f, Active = false,
            Collider = new RectCollider { Width = 10f, Height = 10f }
        });

        var bb = parent.ChildrenBoundingBox;
        Assert.NotNull(bb);
        Assert.True(bb.Value.Right < 100f); // far-away inactive child excluded
    }

    // ── FindChildCollision ────────────────────────────────────────────

    [Fact]
    public void FindChildCollision_FindsOverlappingChild()
    {
        var parent = new Entity();
        var brick = new Entity
        {
            X = 50f, Y = 50f,
            Collider = new RectCollider { Width = 20f, Height = 20f }
        };
        parent.AddChild(brick);

        var ball = new Entity
        {
            X = 55f, Y = 55f,
            Collider = new CircleCollider { Radius = 5f }
        };

        var hit = parent.FindChildCollision(ball, out _);
        Assert.Same(brick, hit);
    }

    [Fact]
    public void FindChildCollision_ReturnsNullWhenNoOverlap()
    {
        var parent = new Entity();
        parent.AddChild(new Entity
        {
            X = 50f, Y = 50f,
            Collider = new RectCollider { Width = 20f, Height = 20f }
        });

        var ball = new Entity
        {
            X = 500f, Y = 500f,
            Collider = new CircleCollider { Radius = 5f }
        };

        var hit = parent.FindChildCollision(ball, out _);
        Assert.Null(hit);
    }

    [Fact]
    public void FindChildCollision_SkipsInactiveChildren()
    {
        var parent = new Entity();
        parent.AddChild(new Entity
        {
            X = 0f, Y = 0f, Active = false,
            Collider = new RectCollider { Width = 100f, Height = 100f }
        });

        var ball = new Entity
        {
            X = 0f, Y = 0f,
            Collider = new CircleCollider { Radius = 5f }
        };

        Assert.Null(parent.FindChildCollision(ball, out _));
    }

    [Fact]
    public void FindChildCollision_ReturnsNullWhenOtherHasNoCollider()
    {
        var parent = new Entity();
        parent.AddChild(new Entity
        {
            X = 0f, Y = 0f,
            Collider = new RectCollider { Width = 100f, Height = 100f }
        });

        var noCollider = new Entity { X = 0f, Y = 0f };

        Assert.Null(parent.FindChildCollision(noCollider, out _));
    }

    // ── Test helpers ──────────────────────────────────────────────────

    private sealed class TestEntity : Entity
    {
        public bool OnUpdateCalled;
        public float LastDeltaTime;

        protected override void OnUpdate(float deltaTime)
        {
            OnUpdateCalled = true;
            LastDeltaTime = deltaTime;
        }
    }

    private sealed class TrackingSprite : Sprite
    {
        public bool DrawCalled;
        public override void Draw(SKCanvas canvas) => DrawCalled = true;
    }
}
