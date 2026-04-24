using SkiaSharp;

using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class ActorTests
{
    // ── Position & World Position ─────────────────────────────────────

    [Fact]
    public void RootActor_WorldPosition_EqualsLocalPosition()
    {
        var actor = new Actor { X = 10f, Y = 20f };
        Assert.Equal(10f, actor.WorldX);
        Assert.Equal(20f, actor.WorldY);
    }

    [Fact]
    public void ChildActor_WorldPosition_IsParentPlusLocal()
    {
        var parent = new Actor { X = 100f, Y = 200f };
        var child = new Actor { X = 10f, Y = 20f };
        parent.Children.Add(child);

        Assert.Equal(110f, child.WorldX);
        Assert.Equal(220f, child.WorldY);
    }

    [Fact]
    public void NestedChildren_WorldPosition_AccumulatesAllAncestors()
    {
        var root = new Actor { X = 10f, Y = 10f };
        var mid = new Actor { X = 20f, Y = 20f };
        var leaf = new Actor { X = 5f, Y = 5f };
        root.Children.Add(mid);
        mid.Children.Add(leaf);

        Assert.Equal(35f, leaf.WorldX);
        Assert.Equal(35f, leaf.WorldY);
    }

    [Fact]
    public void ParentPositionChange_PropagatesWorldToChildren()
    {
        var parent = new Actor { X = 0f, Y = 0f };
        var child = new Actor { X = 10f, Y = 10f };
        parent.Children.Add(child);

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
        parent.Children.Add(child1);
        parent.Children.Add(child2);

        child1.X = 50f;
        Assert.Equal(150f, child1.WorldX);
        Assert.Equal(120f, child2.WorldX); // unchanged
    }

    // ── Rotation ──────────────────────────────────────────────────────

    [Fact]
    public void RootActor_WorldRotation_EqualsLocalRotation()
    {
        var actor = new Actor { Rotation = MathF.PI / 4f };
        // WorldMatrix should rotate (1,0) by 45°
        var mapped = actor.WorldMatrix.MapPoint(1f, 0f);
        float expectedX = MathF.Cos(MathF.PI / 4f);
        float expectedY = MathF.Sin(MathF.PI / 4f);
        Assert.Equal(expectedX, mapped.X, 1e-5f);
        Assert.Equal(expectedY, mapped.Y, 1e-5f);
    }

    [Fact]
    public void ParentRotation_AffectsChildWorldPosition()
    {
        var parent = new Actor
        {
            X = 0f,
            Y = 0f,
            Rotation = MathF.PI / 2f,
        }; // 90°
        var child = new Actor { X = 10f, Y = 0f };
        parent.Children.Add(child);

        // 90° rotation: (10, 0) → (0, 10)
        Assert.Equal(0f, child.WorldX, 1e-4f);
        Assert.Equal(10f, child.WorldY, 1e-4f);
    }

    [Fact]
    public void ParentRotation_AccumulatesChildRotation()
    {
        var parent = new Actor { Rotation = MathF.PI / 4f };
        var child = new Actor { Rotation = MathF.PI / 4f };
        parent.Children.Add(child);

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
        parent.Children.Add(child);

        Assert.Equal(150f, child.WorldX);
        Assert.Equal(150f, child.WorldY);
    }

    // ── Children ──────────────────────────────────────────────────────

    [Fact]
    public void AddChild_SetsParentAndRecalculatesWorld()
    {
        var parent = new Actor { X = 100f, Y = 100f };
        var child = new Actor { X = 10f, Y = 10f };

        parent.Children.Add(child);

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

        parent1.Children.Add(child);
        Assert.Single(parent1.Children);

        parent2.Children.Add(child);
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
        parent.Children.Add(child);

        parent.Children.Remove(child);

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
        parent.Children.Add(active);
        parent.Children.Add(inactive);

        int removed = parent.Children.RemoveInactive();

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

        parent.Children.Add(new Actor());
        parent.Children.Add(new Actor());
        Assert.Equal(2, parent.ChildCount);
    }

    // ── Update ────────────────────────────────────────────────────────

    [Fact]
    public void Update_StepsRigidbody()
    {
        var actor = new Actor
        {
            X = 0f,
            Y = 0f,
            Rigidbody = new Rigidbody2D { VelocityX = 100f, VelocityY = 50f },
        };

        actor.Update(0.5f);

        Assert.Equal(50f, actor.X);
        Assert.Equal(25f, actor.Y);
    }

    [Fact]
    public void Update_RecursesIntoActiveChildren()
    {
        var parent = new Actor();
        var child = new Actor { Rigidbody = new Rigidbody2D { VelocityX = 10f } };
        parent.Children.Add(child);

        parent.Update(1f);

        Assert.Equal(10f, child.X);
    }

    [Fact]
    public void Update_SkipsInactiveActors()
    {
        var actor = new Actor
        {
            Active = false,
            Rigidbody = new Rigidbody2D { VelocityX = 100f },
        };

        actor.Update(1f);

        Assert.Equal(0f, actor.X);
    }

    [Fact]
    public void Update_CallsOnUpdate()
    {
        var actor = new TestEntity();
        actor.Update(0.5f);
        Assert.True(actor.OnUpdateCalled);
        Assert.Equal(0.5f, actor.LastDeltaTime);
    }

    // ── Draw ──────────────────────────────────────────────────────────

    [Fact]
    public void Draw_SkipsInvisibleActor()
    {
        var actor = new TrackingEntity { Visible = false };
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        actor.Draw(surface.Canvas);
        Assert.False(actor.DrawCalled);
    }

    [Fact]
    public void Draw_SkipsInactiveActor()
    {
        var actor = new TrackingEntity { Active = false };
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        actor.Draw(surface.Canvas);
        Assert.False(actor.DrawCalled);
    }

    [Fact]
    public void Draw_CallsOnDraw()
    {
        var actor = new TrackingEntity();
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        actor.Draw(surface.Canvas);
        Assert.True(actor.DrawCalled);
    }

    [Fact]
    public void Draw_RecursesIntoChildren()
    {
        var child = new TrackingEntity();
        var parent = new Actor();
        parent.Children.Add(child);

        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        parent.Draw(surface.Canvas);
        Assert.True(child.DrawCalled);
    }

    // ── Alpha / Cascading Opacity ────────────────────────────────────

    [Fact]
    public void Draw_SkipsZeroAlpha()
    {
        var actor = new TrackingEntity { Alpha = 0f };
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        actor.Draw(surface.Canvas);
        Assert.False(actor.DrawCalled);
    }

    [Fact]
    public void Draw_WorksWithPartialAlpha()
    {
        var actor = new TrackingEntity { Alpha = 0.5f };
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        actor.Draw(surface.Canvas);
        Assert.True(actor.DrawCalled);
    }

    [Fact]
    public void Draw_ChildDrawsWhenParentHasPartialAlpha()
    {
        var parent = new Actor { Alpha = 0.5f };
        var child = new TrackingEntity();
        parent.Children.Add(child);

        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        parent.Draw(surface.Canvas);
        Assert.True(child.DrawCalled);
    }

    [Fact]
    public void Draw_ChildSkippedWhenParentAlphaZero()
    {
        var parent = new Actor { Alpha = 0f };
        var child = new TrackingEntity();
        parent.Children.Add(child);

        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        parent.Draw(surface.Canvas);
        Assert.False(child.DrawCalled);
    }

    [Fact]
    public void Draw_CascadingAlpha_ParentReducesChildOpacity()
    {
        // Parent at 50% alpha, child draws a white pixel.
        // The result should be dimmer than a full-opacity draw.
        var child = new ColoredActor(SKColors.White) { X = 50f, Y = 50f };
        var parent = new Actor { Alpha = 0.5f };
        parent.Children.Add(child);

        // Draw with parent alpha
        using var bmpAlpha = new SKBitmap(100, 100);
        using var canvasAlpha = new SKCanvas(bmpAlpha);
        canvasAlpha.Clear(SKColors.Black);
        parent.Draw(canvasAlpha);
        var pixelAlpha = bmpAlpha.GetPixel(50, 50);

        // Draw without parent alpha (full opacity)
        parent.Alpha = 1f;
        using var bmpFull = new SKBitmap(100, 100);
        using var canvasFull = new SKCanvas(bmpFull);
        canvasFull.Clear(SKColors.Black);
        parent.Draw(canvasFull);
        var pixelFull = bmpFull.GetPixel(50, 50);

        // The alpha version should be darker (lower RGB values)
        Assert.True(
            pixelAlpha.Red < pixelFull.Red,
            $"Expected dimmer pixel with parent alpha: got {pixelAlpha} vs {pixelFull}"
        );
    }

    [Fact]
    public void Draw_CascadingAlpha_NestedHalving()
    {
        // Grandparent 50% → Parent 50% → Child draws white.
        // Effective opacity should be 25%.
        var child = new ColoredActor(SKColors.White) { X = 50f, Y = 50f };
        var parent = new Actor { Alpha = 0.5f };
        var grandparent = new Actor { Alpha = 0.5f };
        parent.Children.Add(child);
        grandparent.Children.Add(parent);

        using var bmp = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.Black);
        grandparent.Draw(canvas);
        var pixel = bmp.GetPixel(50, 50);

        // 25% white on black ≈ RGB(64, 64, 64) — allow tolerance
        Assert.InRange(pixel.Red, 30, 100);
    }

    // ── Collision Helpers ─────────────────────────────────────────────

    [Fact]
    public void Overlaps_ReturnsTrueForOverlappingEntities()
    {
        var a = new Actor
        {
            X = 0f,
            Y = 0f,
            Collider = new CircleCollider { Radius = 10f },
        };
        var b = new Actor
        {
            X = 5f,
            Y = 0f,
            Collider = new CircleCollider { Radius = 10f },
        };

        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_ReturnsFalseForDistantEntities()
    {
        var a = new Actor
        {
            X = 0f,
            Y = 0f,
            Collider = new CircleCollider { Radius = 5f },
        };
        var b = new Actor
        {
            X = 100f,
            Y = 0f,
            Collider = new CircleCollider { Radius = 5f },
        };

        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_ReturnsFalseWithoutCollider()
    {
        var a = new Actor { X = 0f, Y = 0f };
        var b = new Actor
        {
            X = 0f,
            Y = 0f,
            Collider = new CircleCollider { Radius = 10f },
        };

        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void BounceOff_ReflectsVelocity()
    {
        var ball = new Actor
        {
            X = 5f,
            Y = 0f,
            Collider = new CircleCollider { Radius = 10f },
            Rigidbody = new Rigidbody2D { VelocityX = -100f },
        };
        var wall = new Actor
        {
            X = 0f,
            Y = 0f,
            Collider = new RectCollider { Width = 2f, Height = 100f },
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
        var actor = new Actor { X = 10f, Y = 10f };
        Assert.Null(actor.WorldBoundingBox);
    }

    [Fact]
    public void WorldBoundingBox_UsesWorldPosition()
    {
        var parent = new Actor { X = 100f, Y = 100f };
        var child = new Actor
        {
            X = 10f,
            Y = 10f,
            Collider = new RectCollider { Width = 20f, Height = 20f },
        };
        parent.Children.Add(child);

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
        var actor = new Actor
        {
            X = 0f,
            Y = 0f,
            Rotation = MathF.PI / 4f, // 45°
            Collider = new RectCollider { Width = 10f, Height = 10f },
        };

        var bb = actor.WorldBoundingBox;
        Assert.NotNull(bb);
        // 45° rotated 10x10 rect has AABB larger than 10x10
        Assert.True(bb.Value.Width > 10f);
        Assert.True(bb.Value.Height > 10f);
    }

    // ── ChildrenBoundingBox ───────────────────────────────────────────

    [Fact]
    public void ChildrenBoundingBox_NullWithNoChildren()
    {
        var actor = new Actor();
        Assert.Null(actor.ChildrenBoundingBox);
    }

    [Fact]
    public void ChildrenBoundingBox_EnclosesAllActiveChildren()
    {
        var parent = new Actor();
        parent.Children.Add(
            new Actor
            {
                X = 0f,
                Y = 0f,
                Collider = new RectCollider { Width = 10f, Height = 10f },
            }
        );
        parent.Children.Add(
            new Actor
            {
                X = 100f,
                Y = 0f,
                Collider = new RectCollider { Width = 10f, Height = 10f },
            }
        );

        var bb = parent.ChildrenBoundingBox;
        Assert.NotNull(bb);
        Assert.Equal(-5f, bb.Value.Left);
        Assert.Equal(105f, bb.Value.Right);
    }

    [Fact]
    public void ChildrenBoundingBox_SkipsInactiveChildren()
    {
        var parent = new Actor();
        parent.Children.Add(
            new Actor
            {
                X = 0f,
                Y = 0f,
                Collider = new RectCollider { Width = 10f, Height = 10f },
            }
        );
        parent.Children.Add(
            new Actor
            {
                X = 1000f,
                Y = 0f,
                Active = false,
                Collider = new RectCollider { Width = 10f, Height = 10f },
            }
        );

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
            X = 50f,
            Y = 50f,
            Collider = new RectCollider { Width = 20f, Height = 20f },
        };
        parent.Children.Add(brick);

        var ball = new Actor
        {
            X = 55f,
            Y = 55f,
            Collider = new CircleCollider { Radius = 5f },
        };

        var hit = parent.FindChildCollision(ball, out _);
        Assert.Same(brick, hit);
    }

    [Fact]
    public void FindChildCollision_ReturnsNullWhenNoOverlap()
    {
        var parent = new Actor();
        parent.Children.Add(
            new Actor
            {
                X = 50f,
                Y = 50f,
                Collider = new RectCollider { Width = 20f, Height = 20f },
            }
        );

        var ball = new Actor
        {
            X = 500f,
            Y = 500f,
            Collider = new CircleCollider { Radius = 5f },
        };

        var hit = parent.FindChildCollision(ball, out _);
        Assert.Null(hit);
    }

    [Fact]
    public void FindChildCollision_SkipsInactiveChildren()
    {
        var parent = new Actor();
        parent.Children.Add(
            new Actor
            {
                X = 0f,
                Y = 0f,
                Active = false,
                Collider = new RectCollider { Width = 100f, Height = 100f },
            }
        );

        var ball = new Actor
        {
            X = 0f,
            Y = 0f,
            Collider = new CircleCollider { Radius = 5f },
        };

        Assert.Null(parent.FindChildCollision(ball, out _));
    }

    [Fact]
    public void FindChildCollision_ReturnsNullWhenOtherHasNoCollider()
    {
        var parent = new Actor();
        parent.Children.Add(
            new Actor
            {
                X = 0f,
                Y = 0f,
                Collider = new RectCollider { Width = 100f, Height = 100f },
            }
        );

        var noCollider = new Actor { X = 0f, Y = 0f };

        Assert.Null(parent.FindChildCollision(noCollider, out _));
    }

    // ── WorldMatrix ─────────────────────────────────────────────────

    [Fact]
    public void DefaultActor_WorldMatrix_IsIdentity()
    {
        var actor = new Actor();
        Assert.Equal(SKMatrix44.Identity, actor.WorldMatrix);
    }

    [Fact]
    public void WorldMatrix_ComposesParentAndChild()
    {
        var parent = new Actor { X = 100f, Y = 200f };
        var child = new Actor { X = 10f, Y = 20f };
        parent.Children.Add(child);

        // WorldMatrix should map origin to child's world position
        var worldPos = child.WorldMatrix.MapPoint(0f, 0f);
        Assert.Equal(110f, worldPos.X, 1e-3f);
        Assert.Equal(220f, worldPos.Y, 1e-3f);
    }

    [Fact]
    public void LocalMatrix_MapPoint_ReturnsLocalPosition()
    {
        var actor = new Actor { X = 42f, Y = 99f };
        var mapped = actor.LocalMatrix.MapPoint(0f, 0f);
        Assert.Equal(42f, mapped.X, 1e-3f);
        Assert.Equal(99f, mapped.Y, 1e-3f);
    }

    [Fact]
    public void LocalMatrix_WithRotation_RotatesPoints()
    {
        var actor = new Actor
        {
            X = 0f,
            Y = 0f,
            Rotation = MathF.PI / 2f,
        };
        // Rotating (10, 0) by 90° should give (0, 10)
        var mapped = actor.LocalMatrix.MapPoint(10f, 0f);
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

    private sealed class ColoredActor(SKColor color) : Actor
    {
        private readonly SKPaint _paint = new() { Color = color };

        protected override void OnDraw(SKCanvas canvas)
        {
            canvas.DrawRect(-5, -5, 10, 10, _paint);
        }
    }

    // ── CaptureToImage ─────────────────────────────────────────────

    [Fact]
    public void CaptureToImage_ReturnsNonNullImage()
    {
        var actor = new TrackingEntity { X = 50f, Y = 50f };
        using var image = actor.CaptureToImage(100, 100);
        Assert.NotNull(image);
        Assert.Equal(100, image.Width);
        Assert.Equal(100, image.Height);
    }

    // ── SceneNode children with Actor ───────────────────────────────

    [Fact]
    public void Actor_ChildrenBoundingBox_IgnoresNonActorChildren()
    {
        var parent = new Actor();
        parent.Children.Add(new SceneNode()); // non-Actor child
        parent.Children.Add(
            new Actor
            {
                X = 10f,
                Y = 10f,
                Collider = new RectCollider { Width = 20f, Height = 20f },
            }
        );

        var bb = parent.ChildrenBoundingBox;
        Assert.NotNull(bb);
    }

    [Fact]
    public void Actor_FindChildCollision_IgnoresNonActorChildren()
    {
        var parent = new Actor();
        parent.Children.Add(new SceneNode()); // non-Actor child
        parent.Children.Add(
            new Actor
            {
                X = 0f,
                Y = 0f,
                Collider = new RectCollider { Width = 100f, Height = 100f },
            }
        );

        var ball = new Actor
        {
            X = 0f,
            Y = 0f,
            Collider = new CircleCollider { Radius = 5f },
        };

        var hit = parent.FindChildCollision(ball, out _);
        Assert.NotNull(hit);
    }

    [Fact]
    public void Actor_Dump_IncludesName()
    {
        var actor = new Actor
        {
            Name = "testActor",
            X = 5f,
            Y = 10f,
        };
        var dump = actor.Dump();
        Assert.Contains("testActor", dump);
        Assert.Contains("5.0", dump);
    }

    [Fact]
    public void Actor_Dump_IncludesChildTree()
    {
        var parent = new Actor { Name = "parent" };
        var child = new Actor { Name = "child" };
        parent.Children.Add(child);
        var dump = parent.Dump();
        Assert.Contains("parent", dump);
        Assert.Contains("child", dump);
    }

    [Fact]
    public void Actor_Dump_IncludesNonActorChild()
    {
        var parent = new Actor { Name = "parent" };
        var child = new SceneNode { Name = "sceneChild" };
        parent.Children.Add(child);
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
        parent1.Children.Add(child);
        Assert.Equal(110f, child.WorldX, 1e-3f);
        parent2.Children.Add(child);
        Assert.Equal(210f, child.WorldX, 1e-3f);
    }

    [Fact]
    public void Actor_RecalculatesWorld_WhenRemoved()
    {
        var parent = new Actor { X = 100f };
        var child = new Actor { X = 10f };
        parent.Children.Add(child);
        Assert.Equal(110f, child.WorldX, 1e-3f);
        parent.Children.Remove(child);
        Assert.Equal(10f, child.WorldX, 1e-3f);
    }
}
