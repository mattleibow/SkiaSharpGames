using SkiaSharp;
using Xunit;

namespace SkiaSharp.Theatre.Tests;

public class SceneNodeTests
{
    private class TestNode : SceneNode { }

    [Fact]
    public void Name_DefaultIsNull()
    {
        var node = new TestNode();
        Assert.Null(node.Name);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        var node = new TestNode { Name = "root" };
        Assert.Equal("root", node.Name);
    }

    [Fact]
    public void Active_DefaultIsTrue()
    {
        var node = new TestNode();
        Assert.True(node.Active);
    }

    [Fact]
    public void AddChild_SetsParent()
    {
        var parent = new TestNode();
        var child = new TestNode();
        parent.Children.Add(child);
        Assert.Same(parent, child.Parent);
        Assert.Single(parent.Children);
    }

    [Fact]
    public void RemoveChild_ClearsParent()
    {
        var parent = new TestNode();
        var child = new TestNode();
        parent.Children.Add(child);
        parent.Children.Remove(child);
        Assert.Null(child.Parent);
        Assert.Empty(parent.Children);
    }

    [Fact]
    public void AddChild_ReparentsFromOldParent()
    {
        var parent1 = new TestNode();
        var parent2 = new TestNode();
        var child = new TestNode();
        parent1.Children.Add(child);
        parent2.Children.Add(child);
        Assert.Same(parent2, child.Parent);
        Assert.Empty(parent1.Children);
        Assert.Single(parent2.Children);
    }

    [Fact]
    public void InsertChild_InsertsAtIndex()
    {
        var parent = new TestNode();
        var a = new TestNode { Name = "a" };
        var b = new TestNode { Name = "b" };
        var c = new TestNode { Name = "c" };
        parent.Children.Add(a);
        parent.Children.Add(c);
        parent.Children.Insert(1, b);
        Assert.Equal(3, parent.ChildCount);
        Assert.Same(b, parent.Children[1]);
    }

    [Fact]
    public void IndexOf_ReturnsCorrectIndex()
    {
        var parent = new TestNode();
        var a = new TestNode();
        var b = new TestNode();
        parent.Children.Add(a);
        parent.Children.Add(b);
        Assert.Equal(0, parent.Children.IndexOf(a));
        Assert.Equal(1, parent.Children.IndexOf(b));
    }

    [Fact]
    public void IndexOf_ReturnsMinusOneForNonChild()
    {
        var parent = new TestNode();
        var other = new TestNode();
        Assert.Equal(-1, parent.Children.IndexOf(other));
    }

    [Fact]
    public void MoveChildToFront_MovesToEnd()
    {
        var parent = new TestNode();
        var a = new TestNode { Name = "a" };
        var b = new TestNode { Name = "b" };
        var c = new TestNode { Name = "c" };
        parent.Children.Add(a);
        parent.Children.Add(b);
        parent.Children.Add(c);
        parent.Children.MoveToFront(a);
        Assert.Same(b, parent.Children[0]);
        Assert.Same(c, parent.Children[1]);
        Assert.Same(a, parent.Children[2]);
    }

    [Fact]
    public void MoveChildToBack_MovesToStart()
    {
        var parent = new TestNode();
        var a = new TestNode { Name = "a" };
        var b = new TestNode { Name = "b" };
        var c = new TestNode { Name = "c" };
        parent.Children.Add(a);
        parent.Children.Add(b);
        parent.Children.Add(c);
        parent.Children.MoveToBack(c);
        Assert.Same(c, parent.Children[0]);
        Assert.Same(a, parent.Children[1]);
        Assert.Same(b, parent.Children[2]);
    }

    [Fact]
    public void MoveChildUp_SwapsWithNext()
    {
        var parent = new TestNode();
        var a = new TestNode { Name = "a" };
        var b = new TestNode { Name = "b" };
        parent.Children.Add(a);
        parent.Children.Add(b);
        parent.Children.MoveUp(a);
        Assert.Same(b, parent.Children[0]);
        Assert.Same(a, parent.Children[1]);
    }

    [Fact]
    public void MoveChildUp_AtEnd_NoChange()
    {
        var parent = new TestNode();
        var a = new TestNode { Name = "a" };
        var b = new TestNode { Name = "b" };
        parent.Children.Add(a);
        parent.Children.Add(b);
        parent.Children.MoveUp(b);
        Assert.Same(a, parent.Children[0]);
        Assert.Same(b, parent.Children[1]);
    }

    [Fact]
    public void MoveChildDown_SwapsWithPrevious()
    {
        var parent = new TestNode();
        var a = new TestNode { Name = "a" };
        var b = new TestNode { Name = "b" };
        parent.Children.Add(a);
        parent.Children.Add(b);
        parent.Children.MoveDown(b);
        Assert.Same(b, parent.Children[0]);
        Assert.Same(a, parent.Children[1]);
    }

    [Fact]
    public void MoveChildDown_AtStart_NoChange()
    {
        var parent = new TestNode();
        var a = new TestNode { Name = "a" };
        var b = new TestNode { Name = "b" };
        parent.Children.Add(a);
        parent.Children.Add(b);
        parent.Children.MoveDown(a);
        Assert.Same(a, parent.Children[0]);
        Assert.Same(b, parent.Children[1]);
    }

    [Fact]
    public void RemoveInactiveChildren_RemovesOnlyInactive()
    {
        var parent = new TestNode();
        var active = new TestNode { Active = true };
        var inactive = new TestNode { Active = false };
        parent.Children.Add(active);
        parent.Children.Add(inactive);
        int removed = parent.Children.RemoveInactive();
        Assert.Equal(1, removed);
        Assert.Single(parent.Children);
        Assert.Same(active, parent.Children[0]);
    }

    [Fact]
    public void Update_SkipsInactiveNodes()
    {
        bool updated = false;
        var parent = new TestNode { Active = false };
        // If Update is called on inactive parent, children shouldn't be updated
        var child = new UpdatingNode(() => updated = true);
        parent.Children.Add(child);
        parent.Update(1f / 60f);
        Assert.False(updated);
    }

    [Fact]
    public void Update_RecursesIntoChildren()
    {
        bool updated = false;
        var parent = new TestNode();
        var child = new UpdatingNode(() => updated = true);
        parent.Children.Add(child);
        parent.Update(1f / 60f);
        Assert.True(updated);
    }

    [Fact]
    public void Draw_SkipsInactiveNodes()
    {
        bool drawn = false;
        var node = new DrawingNode(() => drawn = true) { Active = false };
        using var bmp = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bmp);
        node.Draw(canvas);
        Assert.False(drawn);
    }

    [Fact]
    public void Draw_RecursesIntoChildren()
    {
        bool drawn = false;
        var parent = new TestNode();
        var child = new DrawingNode(() => drawn = true);
        parent.Children.Add(child);
        using var bmp = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bmp);
        parent.Draw(canvas);
        Assert.True(drawn);
    }

    [Fact]
    public void Dump_IncludesName()
    {
        var node = new TestNode { Name = "myNode" };
        var dump = node.Dump();
        Assert.Contains("myNode", dump);
    }

    [Fact]
    public void Dump_IncludesInactiveFlag()
    {
        var node = new TestNode { Active = false };
        var dump = node.Dump();
        Assert.Contains("INACTIVE", dump);
    }

    [Fact]
    public void Dump_IncludesChildTree()
    {
        var parent = new TestNode { Name = "parent" };
        var child = new TestNode { Name = "child" };
        parent.Children.Add(child);
        var dump = parent.Dump();
        Assert.Contains("parent", dump);
        Assert.Contains("child", dump);
    }

    [Fact]
    public void MoveChildToFront_NonChild_NoOp()
    {
        var parent = new TestNode();
        var a = new TestNode();
        parent.Children.Add(a);
        var other = new TestNode();
        parent.Children.MoveToFront(other);
        Assert.Single(parent.Children);
    }

    [Fact]
    public void MoveChildToBack_NonChild_NoOp()
    {
        var parent = new TestNode();
        var a = new TestNode();
        parent.Children.Add(a);
        var other = new TestNode();
        parent.Children.MoveToBack(other);
        Assert.Single(parent.Children);
    }

    private class UpdatingNode(Action onUpdate) : SceneNode
    {
        protected override void OnUpdate(float deltaTime) => onUpdate();
    }

    private class DrawingNode(Action onDraw) : SceneNode
    {
        protected override void OnDraw(SKCanvas canvas) => onDraw();
    }
}
