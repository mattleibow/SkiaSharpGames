using SkiaSharp;
using SkiaSharp.Theatre;

using Xunit;

namespace SkiaSharp.Theatre.Tests;

/// <summary>Tests for <see cref="Scene"/> default virtual method implementations.</summary>
public class SceneTests
{
    private sealed class DefaultScene : Scene
    {
        protected override void OnDraw(SKCanvas c) { }
    }

    [Fact]
    public void DefaultUpdate_DoesNotThrow()
    {
        var scene = new DefaultScene();
        var ex = Record.Exception(() => scene.Update(0.016f));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnPointerMove_DoesNotThrow()
    {
        var scene = new DefaultScene();
        var ex = Record.Exception(() => scene.OnPointerMove(100f, 200f));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnPointerDown_DoesNotThrow()
    {
        var scene = new DefaultScene();
        var ex = Record.Exception(() => scene.OnPointerDown(100f, 200f));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnPointerUp_DoesNotThrow()
    {
        var scene = new DefaultScene();
        var ex = Record.Exception(() => scene.OnPointerUp(100f, 200f));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnKeyDown_DoesNotThrow()
    {
        var scene = new DefaultScene();
        var ex = Record.Exception(() => scene.OnKeyDown("ArrowLeft"));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnKeyUp_DoesNotThrow()
    {
        var scene = new DefaultScene();
        var ex = Record.Exception(() => scene.OnKeyUp("ArrowLeft"));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnPaused_DoesNotThrow()
    {
        var scene = new DefaultScene();
        var ex = Record.Exception(() => scene.OnPaused());
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnResumed_DoesNotThrow()
    {
        var scene = new DefaultScene();
        var ex = Record.Exception(() => scene.OnResumed());
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnActivating_DoesNotThrow()
    {
        var scene = new DefaultScene();
        var ex = Record.Exception(() => scene.OnActivating());
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnDeactivating_DoesNotThrow()
    {
        var scene = new DefaultScene();
        var ex = Record.Exception(() => scene.OnDeactivating());
        Assert.Null(ex);
    }
}
