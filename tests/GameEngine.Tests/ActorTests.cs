using SkiaSharp;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

public class ActorTests
{
    // ── Position & World Position ─────────────────────────────────────

    [Fact]
    public void RootEntity_WorldPosition_EqualsLocalPosition()
    {
        var entity = new Actor { X = 10f, Y = 20f };
        Assert.Equal(10f, entity.WorldX);
        Assert.Equal(20f, entity.WorldY);
    }

    [Fact]
    public void ChildEntity_WorldPosition_IsParentPlusLocal()
    {
        var parent = new Actor { X = 100f, Y = 200f };
        var child = new Actor { X = 10f, Y = 20f };
        parent.AddChild(child);

        Assert.Equal(110f, child.WorldX);
        Assert.Equal(220f, child.WorldY);
    }

    [Fact]
    public void NestedChildren_WorldPosition_AccumulatesAllAncestors()
    {
        var root = new Actor { X = 10f, Y = 10f };
        var mid = new Actor { X = 20f, Y = 20f };
        var leaf = new Actor { X = 5f, Y = 5f };
        root.AddChild(mid);
        mid.AddChild(leaf);

        Assert.Equal(35f, leaf.WorldX);
        Assert.Equal(35f, leaf.WorldY);
    }

    [Fact]
    public void ParentPositionChange_PropagatesWorldToChildren()
    {
        var parent = new Actor { X = 0f, Y = 0f };
        var child = new Actor { X = 10f, Y = 10f };
        parent.AddChild(child);

        Assert.Equal(10f, child.WorldX);

        parent.X = 50f;
        Assert.Equal(60f, child.WorldX);
        Assert.Equal(10f, child.WorldY);
    }

    [Fact]
    public void ChildPositionChange_UpdatesOwnWorldOnly()
    {
        var parent = new Actor { X = 100f, Y = 200f };
        var child1 = new Actor { X = 10f, Y = 10f };
        var child2 = new Actor { X = 20f, Y = 20f };
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
        var entity = new Actor { Rotation = MathF.PI / 4f };
        // WorldMatrix should rotate (1,0) by 45°
        var mapped = entity.WorldMatrix.MapPoint(1f, 0f);
        float expectedX = MathF.Cos(MathF.PI / 4f);
        float expectedY = MathF.Sin(MathF.PI / 4f);
        Assert.Equal(expectedX, mapped.X, 1e-5f);
        Assert.Equal(expectedY, mapped.Y, 1e-5f);
    }

    [Fact]
    public void ParentRotation_AffectsChildWorldPosition()
    {
        var parent = new Actor { X = 0f, Y = 0f, Rotation = MathF.PI / 2f }; // 90°
        var child = new Actor { X = 10f, Y = 0f };
        parent.AddChild(child);

        // 90° rotation: (10, 0) → (0, 10)
        Assert.Equal(0f, child.WorldX, 1e-4f);
        Assert.Equal(10f, child.WorldY, 1e-4f);
    }

    [Fact]
    public void ParentRotation_AccumulatesChildRotation()
    {
        var parent = new Actor { Rotation = MathF.PI / 4f };
        var child = new Actor { Rotation = MathF.PI / 4f };
        parent.AddChild(child);

        // 45° + 45° = 90°: WorldMatrix should rotate (1,0) to (0,1)
        var mapped = child.WorldMatrix.MapPoint(1f, 0f);
        Assert.Equal(0f, mapped.X, 1e-3f);
        Assert.Equal(1f, mapped.Y, 1e-3f);
    }

    [Fact]
    public void NoParentRotation_UsesSimpleAddition()
    {
        var parent = new Actor { X = 100f, Y = 100f }; // rotation = 0
        var child = new Actor { X = 50f, Y = 50f };
        parent.AddChild(child);

        Assert.Equal(150f, child.WorldX);
        Assert.Equal(150f, child.WorldY);
    }

    // ── Children ──────────────────────────────────────────────────────

    [Fact]
    public void AddChild_SetsParentAndRecalculatesWorld()
    {
        var parent = new Actor { X = 100f, Y = 100f };
        var child = new Actor { X = 10f, Y = 10f };

        parent.AddChild(child);

        Assert.Same(parent, child.Parent);
        Assert.Single(parent.Children);
        Assert.Equal(110f, child.WorldX);
    }

    [Fact]
    public void AddChild_RemovesFromPreviousParent()
    {
        var parent1 = new Actor();
        var parent2 = new Actor { X = 50f, Y = 50f };
        var child = new Actor { X = 10f, Y = 10f };

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
        var parent = new Actor { X = 100f, Y = 100f };
        var child = new Actor { X = 10f, Y = 10f };
        parent.AddChild(child);

        parent.RemoveChild(child);

        Assert.Null(child.Parent);
        Assert.Empty(parent.Children);
        Assert.Equal(10f, child.WorldX); // now root
    }

    [Fact]
    public void RemoveInactiveChildren_RemovesOnlyInactive()
    {
        var parent = new Actor();
        var active = new Actor { Active = true };
        var inactive = new Actor { Active = false };
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
        var parent = new Actor();
        Assert.Equal(0, parent.ChildCount);

        parent.AddChild(new Actor());
        parent.AddChild(new Actor());
        Assert.Equal(2, parent.ChildCount);
    }

    // ── Update ────────────────────────────────────────────────────────

    [Fact]
    public void Update_StepsRigidbody()
    {
        var entity = new Actor
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
        var parent = new Actor();
        var child = new Actor
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
        var entity = new Actor
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
        var entity = new TrackingEntity { Visible = false };
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        entity.Draw(surface.Canvas);
        Assert.False(entity.DrawCalled);
    }

    [Fact]
    public void Draw_SkipsInactiveEntity()
    {
        var entity = new TrackingEntity { Active = false };
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        entity.Draw(surface.Canvas);
        Assert.False(entity.DrawCalled);
    }

    [Fact]
    public void Draw_CallsOnDraw()
    {
        var entity = new TrackingEntity();
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        entity.Draw(surface.Canvas);
        Assert.True(entity.DrawCalled);
    }

    [Fact]
    public void Draw_RecursesIntoChildren()
    {
        var child = new TrackingEntity();
        var parent = new Actor();
        parent.AddChild(child);

        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        parent.Draw(surface.Canvas);
        Assert.True(child.DrawCalled);
    }

    // ── Collision Helpers ─────────────────────────────────────────────

    [Fact]
    public void Overlaps_ReturnsTrueForOverlappingEntities()
    {
        var a = new Actor { X = 0f, Y = 0f, Collider = new CircleCollider { Radius = 10f } };
        var b = new Actor { X = 5f, Y = 0f, Collider = new CircleCollider { Radius = 10f } };

        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_ReturnsFalseForDistantEntities()
    {
        var a = new Actor { X = 0f, Y = 0f, Collider = new CircleCollider { Radius = 5f } };
        var b = new Actor { X = 100f, Y = 0f, Collider = new CircleCollider { Radius = 5f } };

        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_ReturnsFalseWithoutCollider()
    {
        var a = new Actor { X = 0f, Y = 0f };
        var b = new Actor { X = 0f, Y = 0f, Collider = new CircleCollider { Radius = 10f } };

        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void BounceOff_ReflectsVelocity()
    {
        var ball = new Actor
        {
            X = 5f, Y = 0f,
            Collider = new CircleCollider { Radius = 10f },
            Rigidbody = new Rigidbody2D { VelocityX = -100f }
        };
        var wall = new Actor
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
        var a = new Actor { Collider = new CircleCollider { Radius = 10f } };
        var b = new Actor { Collider = new CircleCollider { Radius = 10f } };

        Assert.False(a.BounceOff(b));
    }

    // ── WorldBoundingBox ──────────────────────────────────────────────

    [Fact]
    public void WorldBoundingBox_NullWithoutCollider()
    {
        var entity = new Actor { X = 10f, Y = 10f };
        Assert.Null(entity.WorldBoundingBox);
    }

    [Fact]
    public void WorldBoundingBox_UsesWorldPosition()
    {
        var parent = new Actor { X = 100f, Y = 100f };
        var child = new Actor
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
        var entity = new Actor
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
        var entity = new Actor();
        Assert.Null(entity.ChildrenBoundingBox);
    }

    [Fact]
    public void ChildrenBoundingBox_EnclosesAllActiveChildren()
    {
        var parent = new Actor();
        parent.AddChild(new Actor
        {
            X = 0f, Y = 0f,
            Collider = new RectCollider { Width = 10f, Height = 10f }
        });
        parent.AddChild(new Actor
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
        var parent = new Actor();
        parent.AddChild(new Actor
        {
            X = 0f, Y = 0f,
            Collider = new RectCollider { Width = 10f, Height = 10f }
        });
        parent.AddChild(new Actor
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
        var parent = new Actor();
        var brick = new Actor
        {
            X = 50f, Y = 50f,
            Collider = new RectCollider { Width = 20f, Height = 20f }
        };
        parent.AddChild(brick);

        var ball = new Actor
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
        var parent = new Actor();
        parent.AddChild(new Actor
        {
            X = 50f, Y = 50f,
            Collider = new RectCollider { Width = 20f, Height = 20f }
        });

        var ball = new Actor
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
        var parent = new Actor();
        parent.AddChild(new Actor
        {
            X = 0f, Y = 0f, Active = false,
            Collider = new RectCollider { Width = 100f, Height = 100f }
        });

        var ball = new Actor
        {
            X = 0f, Y = 0f,
            Collider = new CircleCollider { Radius = 5f }
        };

        Assert.Null(parent.FindChildCollision(ball, out _));
    }

    [Fact]
    public void FindChildCollision_ReturnsNullWhenOtherHasNoCollider()
    {
        var parent = new Actor();
        parent.AddChild(new Actor
        {
            X = 0f, Y = 0f,
            Collider = new RectCollider { Width = 100f, Height = 100f }
        });

        var noCollider = new Actor { X = 0f, Y = 0f };

        Assert.Null(parent.FindChildCollision(noCollider, out _));
    }

    // ── WorldMatrix ─────────────────────────────────────────────────

    [Fact]
    public void DefaultEntity_WorldMatrix_IsIdentity()
    {
        var entity = new Actor();
        Assert.Equal(SKMatrix44.Identity, entity.WorldMatrix);
    }

    [Fact]
    public void WorldMatrix_ComposesParentAndChild()
    {
        var parent = new Actor { X = 100f, Y = 200f };
        var child = new Actor { X = 10f, Y = 20f };
        parent.AddChild(child);

        // WorldMatrix should map origin to child's world position
        var worldPos = child.WorldMatrix.MapPoint(0f, 0f);
        Assert.Equal(110f, worldPos.X, 1e-3f);
        Assert.Equal(220f, worldPos.Y, 1e-3f);
    }

    [Fact]
    public void LocalMatrix_MapPoint_ReturnsLocalPosition()
    {
        var entity = new Actor { X = 42f, Y = 99f };
        var mapped = entity.LocalMatrix.MapPoint(0f, 0f);
        Assert.Equal(42f, mapped.X, 1e-3f);
        Assert.Equal(99f, mapped.Y, 1e-3f);
    }

    [Fact]
    public void LocalMatrix_WithRotation_RotatesPoints()
    {
        var entity = new Actor { X = 0f, Y = 0f, Rotation = MathF.PI / 2f };
        // Rotating (10, 0) by 90° should give (0, 10)
        var mapped = entity.LocalMatrix.MapPoint(10f, 0f);
        Assert.Equal(0f, mapped.X, 1e-3f);
        Assert.Equal(10f, mapped.Y, 1e-3f);
    }

    // ── Test helpers ──────────────────────────────────────────────────

    private sealed class TestEntity : Actor
    {
        public bool OnUpdateCalled;
        public float LastDeltaTime;

        protected override void OnUpdate(float deltaTime)
        {
            OnUpdateCalled = true;
            LastDeltaTime = deltaTime;
        }
    }

    private sealed class TrackingEntity : Actor
    {
        public bool DrawCalled;
        protected override void OnDraw(SKCanvas canvas) => DrawCalled = true;
    }

    // ── CaptureToImage ─────────────────────────────────────────────

    [Fact]
    public void CaptureToImage_ReturnsNonNullImage()
    {
        var entity = new TrackingEntity { X = 50f, Y = 50f };
        using var image = entity.CaptureToImage(100, 100);
        Assert.NotNull(image);
        Assert.Equal(100, image.Width);
        Assert.Equal(100, image.Height);
    }

    // ── SceneNode children with Actor ───────────────────────────────

    [Fact]
    public void Actor_ChildrenBoundingBox_IgnoresNonActorChildren()
    {
        var parent = new Actor();
        parent.AddChild(new SceneNode()); // non-Actor child
        parent.AddChild(new Actor
        {
            X = 10f, Y = 10f,
            Collider = new RectCollider { Width = 20f, Height = 20f }
        });

        var bb = parent.ChildrenBoundingBox;
        Assert.NotNull(bb);
    }

    [Fact]
    public void Actor_FindChildCollision_IgnoresNonActorChildren()
    {
        var parent = new Actor();
        parent.AddChild(new SceneNode()); // non-Actor child
        parent.AddChild(new Actor
        {
            X = 0f, Y = 0f,
            Collider = new RectCollider { Width = 100f, Height = 100f }
        });

        var ball = new Actor
        {
            X = 0f, Y = 0f,
            Collider = new CircleCollider { Radius = 5f }
        };

        var hit = parent.FindChildCollision(ball, out _);
        Assert.NotNull(hit);
    }

    [Fact]
    public void Actor_Dump_IncludesName()
    {
        var entity = new Actor { Name = "testActor", X = 5f, Y = 10f };
        var dump = entity.Dump();
        Assert.Contains("testActor", dump);
        Assert.Contains("5.0", dump);
    }

    [Fact]
    public void Actor_Dump_IncludesChildTree()
    {
        var parent = new Actor { Name = "parent" };
        var child = new Actor { Name = "child" };
        parent.AddChild(child);
        var dump = parent.Dump();
        Assert.Contains("parent", dump);
        Assert.Contains("child", dump);
    }

    [Fact]
    public void Actor_Dump_IncludesNonActorChild()
    {
        var parent = new Actor { Name = "parent" };
        var child = new SceneNode { Name = "sceneChild" };
        parent.AddChild(child);
        var dump = parent.Dump();
        Assert.Contains("parent", dump);
        Assert.Contains("sceneChild", dump);
    }

    [Fact]
    public void Actor_RecalculatesWorld_WhenReparented()
    {
        var parent1 = new Actor { X = 100f };
        var parent2 = new Actor { X = 200f };
        var child = new Actor { X = 10f };
        parent1.AddChild(child);
        Assert.Equal(110f, child.WorldX, 1e-3f);
        parent2.AddChild(child);
        Assert.Equal(210f, child.WorldX, 1e-3f);
    }

    [Fact]
    public void Actor_RecalculatesWorld_WhenRemoved()
    {
        var parent = new Actor { X = 100f };
        var child = new Actor { X = 10f };
        parent.AddChild(child);
        Assert.Equal(110f, child.WorldX, 1e-3f);
        parent.RemoveChild(child);
        Assert.Equal(10f, child.WorldX, 1e-3f);
    }
}
