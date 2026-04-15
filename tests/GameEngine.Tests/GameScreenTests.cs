using SkiaSharp;
using SkiaSharpGames.GameEngine;
using Xunit;

namespace SkiaSharpGames.GameEngine.Tests;

/// <summary>Tests for <see cref="GameScreen"/> default virtual method implementations.</summary>
public class GameScreenTests
{
    private sealed class DefaultGameScreen : GameScreen
    {
        public override void Draw(SKCanvas c, int w, int h) { }
    }

    [Fact]
    public void DefaultUpdate_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.Update(0.016f));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnPointerMove_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnPointerMove(100f, 200f));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnPointerDown_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnPointerDown(100f, 200f));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnPointerUp_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnPointerUp(100f, 200f));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnKeyDown_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnKeyDown("ArrowLeft"));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnKeyUp_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnKeyUp("ArrowLeft"));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnPaused_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnPaused());
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnResumed_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnResumed());
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnActivating_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnActivating());
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultOnDeactivating_DoesNotThrow()
    {
        var screen = new DefaultGameScreen();
        var ex = Record.Exception(() => screen.OnDeactivating());
        Assert.Null(ex);
    }
}
